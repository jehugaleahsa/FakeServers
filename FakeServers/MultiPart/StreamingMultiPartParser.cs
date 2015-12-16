using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeServers.MultiPart
{
    internal class StreamingMultiPartParser
    {
        private const int bufferSize = 4096;

        private readonly Stream bodyStream;
        private readonly Encoding encoding;
        private readonly byte[] boundary;
        private readonly byte[] outerBoundary;
        private readonly byte[] dashes;
        private readonly byte[] newLine;
        private readonly Buffer buffer;

        public StreamingMultiPartParser(Stream bodyStream, Encoding encoding, string boundary)
            : this(bodyStream, encoding, boundary, null, new Buffer(encoding))
        {
        }

        private StreamingMultiPartParser(Stream bodyStream, Encoding encoding, string boundary, byte[] outerBoundary, Buffer buffer)
        {
            this.bodyStream = bodyStream;
            this.encoding = encoding;
            this.boundary = encoding.GetBytes("--" + boundary);
            this.outerBoundary = outerBoundary;
            this.dashes = encoding.GetBytes("--");
            this.newLine = encoding.GetBytes("\r\n");
            this.buffer = buffer;
        }

        public event EventHandler<Stream> PreambleFound;

        public event EventHandler<MultiPartSection> SectionFound;

        public event EventHandler<Stream> EpilogueFound;

        public async Task Parse()
        {
            await parseInternal();
        }

        private async Task parseInternal()
        {
            await parsePreamble();
            while (!buffer.IsEmpty && !buffer.StartsWith(dashes))
            {
                if (buffer.StartsWith(newLine))
                {
                    buffer.Shift(newLine.Length);
                }
                await parseSection();
            }
            if (buffer.IsEmpty)
            {
                return;
            }
            if (buffer.StartsWith(newLine, dashes.Length))
            {
                buffer.Shift(dashes.Length + newLine.Length);
            }
            else
            {
                buffer.Shift(dashes.Length);
            }
            await parseEpilogue();
        }

        private async Task parsePreamble()
        {
            MemoryStream content = await parseContent(boundary, newLine);
            if (PreambleFound != null)
            {
                PreambleFound(this, content);
            }
        }

        private async Task parseSection()
        {
            // read headers
            NameValueCollection headers = new NameValueCollection();
            await buffer.Fill(bodyStream);
            while (!buffer.IsEmpty && !buffer.StartsWith(newLine))
            {
                string header = await parseHeader();
                string[] parts = header.Split(new char[] { ':' }, 2);
                if (parts.Length == 2)
                {
                    headers.Add(parts[0].Trim(), parts[1].Trim());
                }
            }
            if (buffer.IsEmpty)
            {
                return;
            }

            // read content
            buffer.Shift(newLine.Length);
            string subBoundary = getSubBoundary(headers);
            if (subBoundary == null)
            {
                MemoryStream content = await parseContent(boundary, newLine);
                if (SectionFound != null)
                {
                    SectionFound(this, new MultiPartSection() { Headers = headers, Content = content });
                }
            }
            else
            {
                StreamingMultiPartParser subParser = new StreamingMultiPartParser(bodyStream, encoding, subBoundary, boundary, buffer);
                if (SectionFound != null)
                {
                    subParser.SectionFound += (o, e) => SectionFound(o, e);
                }
                await subParser.parseInternal();
            }
        }

        private async Task<string> parseHeader()
        {
            MemoryStream headerStream = await parseContent(newLine, null);
            string header = encoding.GetString(headerStream.ToArray());
            return header;
        }

        private static string getSubBoundary(NameValueCollection headers)
        {
            string contentType = headers["Content-Type"];
            if (contentType == null)
            {
                return null;
            }
            if (!contentType.StartsWith("multipart/mixed", StringComparison.CurrentCultureIgnoreCase))
            {
                return null;
            }
            string subBoundary = contentType.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .Select(p => p.Split(new char[] { '=' }, 2))
                .Where(p => p.Length == 2)
                .Where(p => p[0].Equals("boundary", StringComparison.CurrentCultureIgnoreCase))
                .Select(p => p[1])
                .FirstOrDefault();
            return subBoundary;
        }

        private async Task parseEpilogue()
        {
            MemoryStream content = await parseContent(outerBoundary, newLine);
            if (EpilogueFound != null)
            {
                EpilogueFound(this, content);
            }
        }

        private async Task<MemoryStream> parseContent(byte[] boundary, byte[] prefix)
        {
            if (boundary == null)
            {
                return await parseRemainingContent();
            }
            else
            {
                return await parseContentUntilNextBoundary(boundary, prefix);
            }
        }

        private async Task<MemoryStream> parseRemainingContent()
        {
            MemoryStream contentStream = new MemoryStream();
            await buffer.Fill(bodyStream);
            while (!buffer.IsEmpty)
            {
                await buffer.CopyTo(contentStream);
                await buffer.Fill(bodyStream);
            }
            contentStream.Position = 0;
            return contentStream;
        }

        private async Task<MemoryStream> parseContentUntilNextBoundary(byte[] boundary, byte[] prefix)
        {
            MemoryStream contentStream = new MemoryStream();
            await buffer.Fill(bodyStream);
            int boundaryPosition = buffer.Find(boundary);
            while (!buffer.IsEmpty && boundaryPosition == buffer.EndPosition)
            {
                await buffer.CopyHalf(contentStream);
                await buffer.Fill(bodyStream);
                boundaryPosition = buffer.Find(boundary);
            }
            // If the boundary is prefixed with a new line, throw the newline away
            if (prefix != null && buffer.StartsWith(prefix, boundaryPosition - prefix.Length))
            {
                await buffer.CopyTo(contentStream, boundaryPosition - prefix.Length);
                buffer.Shift(boundary.Length + prefix.Length);
            }
            else
            {
                await buffer.CopyTo(contentStream, boundaryPosition);
                buffer.Shift(boundary.Length);
            }
            contentStream.Position = 0;
            return contentStream;
        }

        private static int findSequence(
            byte[] list1, int first1, int past1,
            byte[] list2, int first2, int past2)
        {
            int count1 = past1 - first1;
            int count2 = past2 - first2;
            while (count2 <= count1)
            {
                int middle1 = first1;
                int middle2 = first2;
                while (middle2 != past2 && list1[middle1] == list2[middle2])
                {
                    ++middle1;
                    ++middle2;
                }
                if (middle2 == past2)
                {
                    return first1;
                }
                ++first1;
                --count1;
            }
            return past1;
        }

        private static bool startsWith(
            byte[] list1, int first1, int past1,
            byte[] list2, int first2, int past2)
        {
            int count1 = past1 - first1;
            int count2 = past2 - first2;
            if (count2 > count1)
            {
                return false;
            }
            int middle1 = first1;
            int middle2 = first2;
            while (middle2 != past2 && list1[middle1] == list2[middle2])
            {
                ++middle1;
                ++middle2;
            }
            if (middle2 == past2)
            {
                return true;
            }
            return false;
        }

        private class Buffer
        {
            private readonly Encoding encoding;
            private readonly byte[] buffer;

            public Buffer(Encoding encoding)
            {
                this.encoding = encoding;
                this.buffer = new byte[bufferSize + bufferSize];
            }

            public byte[] Data
            {
                get { return buffer; }
            }

            public int Position { get; set; }

            public int EndPosition { get; set; }

            public bool IsEmpty
            {
                get { return Position == 0 && Position == EndPosition; }
            }

            public void Shift(int shift)
            {
                if (shift == 0)
                {
                    return;
                }
                if (shift >= EndPosition - Position)
                {
                    Position = 0;
                    EndPosition = 0;
                    return;
                }
                Position += shift;
                Array.Copy(buffer, Position, buffer, 0, EndPosition - Position);
                Position = 0;
                EndPosition -= shift;
            }

            public async Task Fill(Stream sourceStream)
            {
                int bytesRead = await sourceStream.ReadAsync(buffer, EndPosition, buffer.Length - EndPosition);
                EndPosition += bytesRead;
            }

            public async Task CopyHalf(Stream destinationStream)
            {
                await CopyTo(destinationStream, buffer.Length / 2);
            }

            public async Task CopyTo(Stream destinationStream)
            {
                await CopyTo(destinationStream, buffer.Length);
            }

            public async Task CopyTo(Stream destinationStream, int maxLength)
            {
                // We only copy half of a buffer at a time
                maxLength = Math.Min(maxLength, EndPosition - Position);
                await destinationStream.WriteAsync(buffer, Position, maxLength);
                Shift(Position + maxLength);
            }

            public int Find(byte[] boundary)
            {
                int position = StreamingMultiPartParser.findSequence(
                    buffer, Position, EndPosition,
                    boundary, 0, boundary.Length);
                return position;
            }

            public bool StartsWith(byte[] prefix, int position = 0)
            {
                if (position < Position || position > EndPosition - prefix.Length)
                {
                    return false;
                }
                bool result = StreamingMultiPartParser.startsWith(
                    buffer, position, EndPosition,
                    prefix, 0, prefix.Length);
                return result;
            }

            public override string ToString()
            {
                string encoded = encoding.GetString(buffer, Position, EndPosition - Position);
                return encoded;
            }
        }
    }
}
