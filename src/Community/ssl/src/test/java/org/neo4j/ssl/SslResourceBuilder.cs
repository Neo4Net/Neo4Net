using System.Collections.Generic;

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
namespace Neo4Net.Ssl
{

	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.ssl.SslResourceBuilder.SignedBy.CA;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.ssl.SslResourceBuilder.SignedBy.SELF;

	/// <summary>
	/// This builder has a finite set of pre-generated resource
	/// keys and certificates which can be utilized in tests.
	/// </summary>
	public class SslResourceBuilder
	{
		 private const string CA_CERTIFICATE_NAME = "cluster.crt";

		 private const string PRIVATE_KEY_NAME = "private.key";
		 private const string PUBLIC_CERT_NAME = "public.crt";

		 private const string SELF_SIGNED_NAME = "selfsigned.crt";
		 private const string REVOKED_NAME = "revoked.crl";
		 private const string CA_SIGNED_NAME = "casigned.crt";

		 private const string TRUSTED_DIR_NAME = "trusted";
		 private const string REVOKED_DIR_NAME = "revoked";

		 private const string CA_BASE_PATH = "test-certificates/ca/";
		 private const string SERVERS_BASE_PATH = "test-certificates/servers/";

		 private readonly int _keyId;

		 internal sealed class SignedBy
		 {
			  public static readonly SignedBy Self = new SignedBy( "Self", InnerEnum.Self, SELF_SIGNED_NAME );
			  public static readonly SignedBy Ca = new SignedBy( "Ca", InnerEnum.Ca, CA_SIGNED_NAME );

			  private static readonly IList<SignedBy> valueList = new List<SignedBy>();

			  static SignedBy()
			  {
				  valueList.Add( Self );
				  valueList.Add( Ca );
			  }

			  public enum InnerEnum
			  {
				  Self,
				  Ca
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  internal Private readonly;

			  internal SignedBy( string name, InnerEnum innerEnum, string resourceName )
			  {
					this._resourceName = resourceName;

				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  public java.net.URL KeyId( int keyId )
			  {
					return Resource( _resourceName, keyId );
			  }

			 public static IList<SignedBy> values()
			 {
				 return valueList;
			 }

			 public int ordinal()
			 {
				 return ordinalValue;
			 }

			 public override string ToString()
			 {
				 return nameValue;
			 }

			 public static SignedBy valueOf( string name )
			 {
				 foreach ( SignedBy enumInstance in SignedBy.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

		 private readonly SignedBy _signedBy;

		 private bool _trustSignedByCA;
		 private ISet<int> _trusted = new HashSet<int>();
		 private ISet<int> _revoked = new HashSet<int>();

		 private FileSystemAbstraction _fsa = new DefaultFileSystemAbstraction();

		 private SslResourceBuilder( int keyId, SignedBy signedBy )
		 {
			  this._keyId = keyId;
			  this._signedBy = signedBy;
		 }

		 public static SslResourceBuilder SelfSignedKeyId( int keyId )
		 {
			  return new SslResourceBuilder( keyId, SELF );
		 }

		 public static SslResourceBuilder CaSignedKeyId( int keyId )
		 {
			  return new SslResourceBuilder( keyId, CA );
		 }

		 public virtual SslResourceBuilder TrustKeyId( int keyId )
		 {
			  _trusted.Add( keyId );
			  return this;
		 }

		 public virtual SslResourceBuilder TrustSignedByCA()
		 {
			  this._trustSignedByCA = true;
			  return this;
		 }

		 public virtual SslResourceBuilder Revoke( int keyId )
		 {
			  _revoked.Add( keyId );
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public SslResource install(java.io.File targetDirectory) throws java.io.IOException
		 public virtual SslResource Install( File targetDirectory )
		 {
			  return Install( targetDirectory, CA_CERTIFICATE_NAME );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public SslResource install(java.io.File targetDirectory, String trustedFileName) throws java.io.IOException
		 public virtual SslResource Install( File targetDirectory, string trustedFileName )
		 {
			  File targetKey = new File( targetDirectory, PRIVATE_KEY_NAME );
			  File targetCertificate = new File( targetDirectory, PUBLIC_CERT_NAME );
			  File targetTrusted = new File( targetDirectory, TRUSTED_DIR_NAME );
			  File targetRevoked = new File( targetDirectory, REVOKED_DIR_NAME );

			  _fsa.mkdir( targetTrusted );
			  _fsa.mkdir( targetRevoked );

			  foreach ( int trustedKeyId in _trusted )
			  {
					File targetTrustedCertificate = new File( targetTrusted, trustedKeyId.ToString() + ".crt" );
					Copy( Resource( SELF_SIGNED_NAME, trustedKeyId ), targetTrustedCertificate );
			  }

			  foreach ( int revokedKeyId in _revoked )
			  {
					File targetRevokedCRL = new File( targetRevoked, revokedKeyId.ToString() + ".crl" );
					Copy( Resource( REVOKED_NAME, revokedKeyId ), targetRevokedCRL );
			  }

			  if ( _trustSignedByCA )
			  {
					File targetTrustedCertificate = new File( targetTrusted, trustedFileName );
					Copy( Resource( trustedFileName ), targetTrustedCertificate );
			  }

			  Copy( Resource( PRIVATE_KEY_NAME, _keyId ), targetKey );
			  Copy( _signedBy.keyId( _keyId ), targetCertificate );

			  return new SslResource( targetKey, targetCertificate, targetTrusted, targetRevoked );
		 }

		 private static URL Resource( string filename, int keyId )
		 {
			  return typeof( SslResourceBuilder ).getResource( SERVERS_BASE_PATH + keyId.ToString() + "/" + filename );
		 }

		 private static URL Resource( string filename )
		 {
			  return typeof( SslResourceBuilder ).getResource( CA_BASE_PATH + filename );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void copy(java.net.URL in, java.io.File outFile) throws java.io.IOException
		 private void Copy( URL @in, File outFile )
		 {
			  using ( Stream @is = @in.openStream(), Stream os = _fsa.openAsOutputStream(outFile, false) )
			  {
					while ( @is.available() > 0 )
					{
						 sbyte[] buf = new sbyte[8192];
						 int nBytes = @is.Read( buf, 0, buf.Length );
						 os.Write( buf, 0, nBytes );
					}
			  }
		 }
	}

}