using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Ssl
{
	using X500Name = org.bouncycastle.asn1.x500.X500Name;
	using Extension = org.bouncycastle.asn1.x509.Extension;
	using GeneralName = org.bouncycastle.asn1.x509.GeneralName;
	using GeneralNames = org.bouncycastle.asn1.x509.GeneralNames;
	using X509CertificateHolder = org.bouncycastle.cert.X509CertificateHolder;
	using X509v3CertificateBuilder = org.bouncycastle.cert.X509v3CertificateBuilder;
	using JcaX509CertificateConverter = org.bouncycastle.cert.jcajce.JcaX509CertificateConverter;
	using JcaX509v3CertificateBuilder = org.bouncycastle.cert.jcajce.JcaX509v3CertificateBuilder;
	using BouncyCastleProvider = org.bouncycastle.jce.provider.BouncyCastleProvider;
	using ContentSigner = org.bouncycastle.@operator.ContentSigner;
	using OperatorCreationException = org.bouncycastle.@operator.OperatorCreationException;
	using JcaContentSignerBuilder = org.bouncycastle.@operator.jcajce.JcaContentSignerBuilder;
	using PemObject = org.bouncycastle.util.io.pem.PemObject;
	using PemReader = org.bouncycastle.util.io.pem.PemReader;
	using PemWriter = org.bouncycastle.util.io.pem.PemWriter;


	/// <summary>
	/// Public Key Infrastructure utilities, e.g. generating/loading keys and certificates.
	/// </summary>
	public class PkiUtils
	{
		 /* Generating SSL certificates takes a long time.
		  * This non-official setting allows us to use a fast source of randomness when running tests */
		 private static readonly bool _useInsecureCertificateGeneration = Boolean.getBoolean( "org.Neo4Net.useInsecureCertificateGeneration" );
		 public const string CERTIFICATE_TYPE = "X.509";
		 private const string DEFAULT_ENCRYPTION = "RSA";
		 private readonly SecureRandom _random;
		 /// <summary>
		 /// Current time minus 1 year, just in case software clock goes back due to time synchronization </summary>
		 private static readonly DateTime _notBefore = new DateTime( DateTimeHelper.CurrentUnixTimeMillis() - 86400000L * 365 );
		 /// <summary>
		 /// The maximum possible value in X.509 specification: 9999-12-31 23:59:59 </summary>
		 private static readonly DateTime _notAfter = new DateTime( 253402300799000L );
		 private static readonly Provider _provider = new BouncyCastleProvider();

		 private static volatile bool _cleanupRequired = true;

		 static PkiUtils()
		 {
			  Security.addProvider( _provider );
		 }

		 public PkiUtils()
		 {
			  _random = _useInsecureCertificateGeneration ? new InsecureRandom() : new SecureRandom();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void createSelfSignedCertificate(java.io.File certificatePath, java.io.File privateKeyPath, String hostName) throws java.security.GeneralSecurityException, java.io.IOException, org.bouncycastle.operator.OperatorCreationException
		 public virtual void CreateSelfSignedCertificate( File certificatePath, File privateKeyPath, string hostName )
		 {
			  InstallCleanupHook( certificatePath, privateKeyPath );
			  KeyPairGenerator keyGen = KeyPairGenerator.getInstance( DEFAULT_ENCRYPTION );
			  keyGen.initialize( 2048, _random );
			  KeyPair keypair = keyGen.generateKeyPair();

			  // Prepare the information required for generating an X.509 certificate.
			  X500Name owner = new X500Name( "CN=" + hostName );
			  X509v3CertificateBuilder builder = new JcaX509v3CertificateBuilder( owner, new BigInteger( 64, _random ), _notBefore, _notAfter, owner, keypair.Public );

			  // Subject alternative name (part of SNI extension, used for hostname verification)
			  GeneralNames subjectAlternativeName = new GeneralNames( new GeneralName( GeneralName.dNSName, hostName ) );
			  builder.addExtension( Extension.subjectAlternativeName, false, subjectAlternativeName );

			  PrivateKey privateKey = keypair.Private;
			  ContentSigner signer = ( new JcaContentSignerBuilder( "SHA512WithRSAEncryption" ) ).build( privateKey );
			  X509CertificateHolder certHolder = builder.build( signer );
			  X509Certificate cert = ( new JcaX509CertificateConverter() ).setProvider(_provider).getCertificate(certHolder);

			  //check so that cert is valid
			  cert.verify( keypair.Public );

			  //write to disk
			  WritePem( "CERTIFICATE", cert.Encoded, certificatePath );
			  WritePem( "PRIVATE KEY", privateKey.Encoded, privateKeyPath );
			  // Mark as done so we don't clean up certificates
			  _cleanupRequired = false;
		 }

		 /// <summary>
		 /// Makes sure to delete partially generated certificates. Does nothing if both certificate and private key have
		 /// been generated successfully.
		 /// 
		 /// The hook should only be installed prior to generation of self-signed certificate, and not if certificates
		 /// already exist.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static void installCleanupHook(final java.io.File certificatePath, final java.io.File privateKeyPath)
		 private static void InstallCleanupHook( File certificatePath, File privateKeyPath )
		 {
			  Runtime.Runtime.addShutdownHook(new Thread(() =>
			  {
				if ( _cleanupRequired )
				{
					 Console.Error.WriteLine( "Cleaning up partially generated self-signed certificate..." );

					 if ( certificatePath.exists() )
					 {
						  certificatePath.delete();
					 }

					 if ( privateKeyPath.exists() )
					 {
						  privateKeyPath.delete();
					 }
				}
			  }));
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.security.cert.X509Certificate[] loadCertificates(java.io.File certFile) throws java.security.cert.CertificateException, java.io.IOException
		 public virtual X509Certificate[] LoadCertificates( File certFile )
		 {
			  CertificateFactory certFactory = CertificateFactory.getInstance( CERTIFICATE_TYPE );
			  ICollection<X509Certificate> certificates = new LinkedList<X509Certificate>();

			  using ( PemReader r = new PemReader( new StreamReader( certFile ) ) )
			  {
					for ( PemObject pemObject = r.readPemObject(); pemObject != null; pemObject = r.readPemObject() )
					{
						 sbyte[] encodedCert = pemObject.Content;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Collection<? extends java.security.cert.X509Certificate> loadedCertificates = (java.util.Collection<java.security.cert.X509Certificate>) certFactory.generateCertificates(new java.io.ByteArrayInputStream(encodedCert));
						 ICollection<X509Certificate> loadedCertificates = ( ICollection<X509Certificate> ) certFactory.generateCertificates( new MemoryStream( encodedCert ) );
						 certificates.addAll( loadedCertificates );
					}
			  }

			  if ( certificates.Count == 0 )
			  {
					// Ok, failed to read as PEM file, try and read it as raw binary certificate
					using ( FileStream @in = new FileStream( certFile, FileMode.Open, FileAccess.Read ) )
					{
						 certificates = ( ICollection<X509Certificate> ) certFactory.generateCertificates( @in );
					}
			  }

			  return certificates.toArray( new X509Certificate[certificates.Count] );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.security.PrivateKey loadPrivateKey(java.io.File privateKeyFile) throws java.io.IOException, java.security.NoSuchAlgorithmException, java.security.spec.InvalidKeySpecException
		 public virtual PrivateKey LoadPrivateKey( File privateKeyFile )
		 {
			  using ( PemReader r = new PemReader( new StreamReader( privateKeyFile ) ) )
			  {
					PemObject pemObject = r.readPemObject();
					if ( pemObject != null )
					{
						 sbyte[] encodedKey = pemObject.Content;
						 KeySpec keySpec = new PKCS8EncodedKeySpec( encodedKey );
						 try
						 {
							  return KeyFactory.getInstance( "RSA" ).generatePrivate( keySpec );
						 }
						 catch ( InvalidKeySpecException )
						 {
							  try
							  {
									return KeyFactory.getInstance( "DSA" ).generatePrivate( keySpec );
							  }
							  catch ( InvalidKeySpecException )
							  {
									try
									{
										 return KeyFactory.getInstance( "EC" ).generatePrivate( keySpec );
									}
									catch ( InvalidKeySpecException e )
									{
										 throw new InvalidKeySpecException( "Neither RSA, DSA nor EC worked", e );
									}
							  }
						 }
					}
			  }

			  // Ok, failed to read as PEM file, try and read it as a raw binary private key
			  using ( DataInputStream @in = new DataInputStream( new FileStream( privateKeyFile, FileMode.Open, FileAccess.Read ) ) )
			  {
					sbyte[] keyBytes = new sbyte[( int ) privateKeyFile.length()];
					@in.readFully( keyBytes );

					KeySpec keySpec = new PKCS8EncodedKeySpec( keyBytes );

					return KeyFactory.getInstance( DEFAULT_ENCRYPTION ).generatePrivate( keySpec );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writePem(String type, byte[] encodedContent, java.io.File path) throws java.io.IOException
		 private void WritePem( string type, sbyte[] encodedContent, File path )
		 {
			  path.ParentFile.mkdirs();
			  using ( PemWriter writer = new PemWriter( new StreamWriter( path ) ) )
			  {
					writer.writeObject( new PemObject( type, encodedContent ) );
					writer.flush();
			  }
			  path.setReadable( false, false );
			  path.setWritable( false, false );
			  path.Readable = true;
			  path.Writable = true;
		 }
	}

}