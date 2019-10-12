/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Org.Neo4j.Ssl
{
	using SelfSignedCertificate = io.netty.handler.ssl.util.SelfSignedCertificate;
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using FileUtils = Org.Neo4j.Io.fs.FileUtils;
	using Inject = Org.Neo4j.Test.extension.Inject;
	using TestDirectoryExtension = Org.Neo4j.Test.extension.TestDirectoryExtension;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.notNullValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(TestDirectoryExtension.class) class TestSslCertificateFactory
	internal class TestSslCertificateFactory
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.test.rule.TestDirectory testDirectory;
		 private TestDirectory _testDirectory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCreateASelfSignedCertificate() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldCreateASelfSignedCertificate()
		 {
			  // Given
			  PkiUtils sslFactory = new PkiUtils();
			  File cPath = new File( _testDirectory.directory(), "certificate" );
			  File pkPath = new File( _testDirectory.directory(), "key" );

			  // When
			  sslFactory.CreateSelfSignedCertificate( cPath, pkPath, "myhost" );

			  // Then
			  // Attempt to load certificate
			  Certificate[] certificates = sslFactory.LoadCertificates( cPath );
			  assertThat( certificates.Length, @is( greaterThan( 0 ) ) );

			  // Attempt to load private key
			  PrivateKey pk = sslFactory.LoadPrivateKey( pkPath );
			  assertThat( pk, notNullValue() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldLoadPEMCertificates() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldLoadPEMCertificates()
		 {
			  // Given
			  SelfSignedCertificate cert = new SelfSignedCertificate( "example.com" );
			  PkiUtils certs = new PkiUtils();

			  File pemCertificate = cert.certificate();

			  // When
			  Certificate[] certificates = certs.LoadCertificates( pemCertificate );

			  // Then
			  assertThat( certificates.Length, equalTo( 1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldLoadPEMPrivateKey() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldLoadPEMPrivateKey()
		 {
			  // Given
			  SelfSignedCertificate cert = new SelfSignedCertificate( "example.com" );
			  PkiUtils certs = new PkiUtils();

			  File privateKey = cert.privateKey();

			  // When
			  PrivateKey pk = certs.LoadPrivateKey( privateKey );

			  // Then
			  assertNotNull( pk );
		 }

		 /// <summary>
		 /// For backwards-compatibility reasons, we support both PEM-encoded certificates *and* raw binary files containing
		 /// the certificate data.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldLoadBinaryCertificates() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldLoadBinaryCertificates()
		 {
			  // Given
			  SelfSignedCertificate cert = new SelfSignedCertificate( "example.com" );
			  PkiUtils certs = new PkiUtils();

			  File cPath = _testDirectory.file( "certificate" );
			  assertTrue( cPath.createNewFile() );
			  sbyte[] raw = certs.LoadCertificates( cert.certificate() )[0].Encoded;

			  using ( FileChannel ch = FileChannel.open( cPath.toPath(), WRITE ) )
			  {
					FileUtils.writeAll( ch, ByteBuffer.wrap( raw ) );
			  }

			  // When
			  Certificate[] certificates = certs.LoadCertificates( cPath );

			  // Then
			  assertThat( certificates.Length, equalTo( 1 ) );
		 }

		 /// <summary>
		 /// For backwards-compatibility reasons, we support both PEM-encoded private keys *and* raw binary files containing
		 /// the private key data
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldLoadBinaryPrivateKey() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldLoadBinaryPrivateKey()
		 {
			  // Given
			  SelfSignedCertificate cert = new SelfSignedCertificate( "example.com" );
			  PkiUtils certs = new PkiUtils();

			  File keyFile = _testDirectory.file( "certificate" );
			  assertTrue( keyFile.createNewFile() );
			  sbyte[] raw = certs.LoadPrivateKey( cert.privateKey() ).Encoded;

			  using ( FileChannel ch = FileChannel.open( keyFile.toPath(), WRITE ) )
			  {
					FileUtils.writeAll( ch, ByteBuffer.wrap( raw ) );
			  }

			  // When
			  PrivateKey pk = certs.LoadPrivateKey( keyFile );

			  // Then
			  assertNotNull( pk );
		 }
	}

}