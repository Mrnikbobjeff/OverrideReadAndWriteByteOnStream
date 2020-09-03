using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;
using OverrideReadWriteByteOnStream;
using System.IO;

namespace OverrideReadWriteByteOnStream.Test
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
    {

        [TestMethod]
        public void NoDiagnostic_EmptyText()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }
        

        [TestMethod]
        public void SingleDiagnostic_ReadByteNotPresent()
        {
            var test = @"
    using System.IO;

    namespace ConsoleApplication1
    {
        class TypeName : Stream
        {
            public override bool CanRead { get; }
            public override bool CanSeek { get; }
            public override bool CanWrite { get; }
            public override long Length { get; }
            public override long Position { get; set; }

            public override void Flush()
            {
                throw new NotImplementedException();
            }
            public override byte ReadByte() {return 1;}
            public override int Read(byte[] buffer, int offset, int count)
            {
                throw new NotImplementedException();
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotImplementedException();
            }

            public override void SetLength(long value)
            {
                throw new NotImplementedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotImplementedException();
            }
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = "OverrideReadWriteByteOnStream",
                Message = String.Format("Method '{0}' has no implementation in derived type", "WriteByte"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 6, 15)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }
        [TestMethod]
        public void SingleDiagnostic_WriteByteNotPresent()
        {
            var test = @"
    using System.IO;

    namespace ConsoleApplication1
    {
        class TypeName : Stream
        {
            public override bool CanRead { get; }
            public override bool CanSeek { get; }
            public override bool CanWrite { get; }
            public override long Length { get; }
            public override long Position { get; set; }

            public override void Flush()
            {
                throw new NotImplementedException();
            }
            public override void WriteByte(byte b) {}
            public override int Read(byte[] buffer, int offset, int count)
            {
                throw new NotImplementedException();
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotImplementedException();
            }

            public override void SetLength(long value)
            {
                throw new NotImplementedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotImplementedException();
            }
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = "OverrideReadWriteByteOnStream",
                Message = String.Format("Method '{0}' has no implementation in derived type", "ReadByte"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 6, 15)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new OverrideReadWriteByteOnStreamAnalyzer();
        }
    }
}
