using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.Server.security.enterprise.auth
{
	using HashedCredentialsMatcher = org.apache.shiro.authc.credential.HashedCredentialsMatcher;
	using RandomNumberGenerator = org.apache.shiro.crypto.RandomNumberGenerator;
	using SecureRandomNumberGenerator = org.apache.shiro.crypto.SecureRandomNumberGenerator;
	using SimpleHash = org.apache.shiro.crypto.hash.SimpleHash;
	using ByteSource = org.apache.shiro.util.ByteSource;


	public class SecureHasher
	{
		 // TODO: Do we need to make this configurable?
		 private const string HASH_ALGORITHM = "SHA-256";
		 private const int HASH_ITERATIONS = 1024;
		 private const int SALT_BYTES_SIZE = 32;

		 private RandomNumberGenerator _randomNumberGenerator;
		 private HashedCredentialsMatcher _hashedCredentialsMatcher;
		 private IDictionary<int, HashedCredentialsMatcher> _hashedCredentialsMatchers;

		 private RandomNumberGenerator RandomNumberGenerator
		 {
			 get
			 {
				  if ( _randomNumberGenerator == null )
				  {
						_randomNumberGenerator = new SecureRandomNumberGenerator();
				  }
   
				  return _randomNumberGenerator;
			 }
		 }

		 public virtual SimpleHash Hash( sbyte[] source )
		 {
			  ByteSource salt = GenerateRandomSalt( SALT_BYTES_SIZE );
			  return new SimpleHash( HASH_ALGORITHM, source, salt, HASH_ITERATIONS );
		 }

		 public virtual HashedCredentialsMatcher HashedCredentialsMatcher
		 {
			 get
			 {
				  if ( _hashedCredentialsMatcher == null )
				  {
						_hashedCredentialsMatcher = new HashedCredentialsMatcher( HASH_ALGORITHM );
						_hashedCredentialsMatcher.HashIterations = HASH_ITERATIONS;
				  }
   
				  return _hashedCredentialsMatcher;
			 }
		 }

		 public virtual HashedCredentialsMatcher GetHashedCredentialsMatcherWithIterations( int iterations )
		 {
			  if ( _hashedCredentialsMatchers == null )
			  {
					_hashedCredentialsMatchers = new Dictionary<int, HashedCredentialsMatcher>();
			  }

			  HashedCredentialsMatcher matcher = _hashedCredentialsMatchers[iterations];
			  if ( matcher == null )
			  {
					matcher = new HashedCredentialsMatcher( HASH_ALGORITHM );
					matcher.HashIterations = iterations;
					_hashedCredentialsMatchers[iterations] = matcher;
			  }

			  return matcher;
		 }

		 private ByteSource GenerateRandomSalt( int bytesSize )
		 {
			  return RandomNumberGenerator.NextBytes( bytesSize );
		 }
	}

}