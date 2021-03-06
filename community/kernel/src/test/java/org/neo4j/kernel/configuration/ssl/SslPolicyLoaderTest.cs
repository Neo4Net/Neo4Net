﻿using System;
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
namespace Org.Neo4j.Kernel.configuration.ssl
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using FileUtils = Org.Neo4j.Io.fs.FileUtils;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using PkiUtils = Org.Neo4j.Ssl.PkiUtils;
	using SslPolicy = Org.Neo4j.Ssl.SslPolicy;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.neo4j_home;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;

	public class SslPolicyLoaderTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public TestDirectory TestDirectory = TestDirectory.testDirectory();

		 private File _home;
		 private File _publicCertificateFile;
		 private File _privateKeyFile;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  _home = TestDirectory.directory( "home" );
			  File baseDir = new File( _home, "certificates/default" );
			  _publicCertificateFile = new File( baseDir, "public.crt" );
			  _privateKeyFile = new File( baseDir, "private.key" );

			  ( new PkiUtils() ).createSelfSignedCertificate(_publicCertificateFile, _privateKeyFile, "localhost");

			  File trustedDir = new File( baseDir, "trusted" );
			  trustedDir.mkdir();
			  FileUtils.copyFile( _publicCertificateFile, new File( trustedDir, "public.crt" ) );
			  ( new File( baseDir, "revoked" ) ).mkdir();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLoadBaseCryptographicObjects() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLoadBaseCryptographicObjects()
		 {
			  // given
			  IDictionary<string, string> @params = stringMap();

			  SslPolicyConfig policyConfig = new SslPolicyConfig( "default" );

			  @params[neo4j_home.name()] = _home.AbsolutePath;
			  @params[policyConfig.BaseDirectory.name()] = "certificates/default";
			  Config config = Config.defaults( @params );

			  // when
			  SslPolicyLoader sslPolicyLoader = SslPolicyLoader.Create( config, NullLogProvider.Instance );

			  // then
			  SslPolicy sslPolicy = sslPolicyLoader.GetPolicy( "default" );
			  assertNotNull( sslPolicy );
			  assertNotNull( sslPolicy.PrivateKey() );
			  assertNotNull( sslPolicy.CertificateChain() );
			  assertNotNull( sslPolicy.NettyClientContext() );
			  assertNotNull( sslPolicy.NettyServerContext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldComplainIfMissingPrivateKey()
		 public virtual void ShouldComplainIfMissingPrivateKey()
		 {
			  ShouldComplainIfMissingFile( _privateKeyFile );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldComplainIfMissingPublicCertificate()
		 public virtual void ShouldComplainIfMissingPublicCertificate()
		 {
			  ShouldComplainIfMissingFile( _publicCertificateFile );
		 }

		 private void ShouldComplainIfMissingFile( File file )
		 {
			  // given
			  FileUtils.deleteFile( file );

			  IDictionary<string, string> @params = stringMap();

			  SslPolicyConfig policyConfig = new SslPolicyConfig( "default" );

			  @params[neo4j_home.name()] = _home.AbsolutePath;
			  @params[policyConfig.BaseDirectory.name()] = "certificates/default";

			  Config config = Config.defaults( @params );

			  // when
			  try
			  {
					SslPolicyLoader.Create( config, NullLogProvider.Instance );
					fail();
			  }
			  catch ( Exception e )
			  {
					assertTrue( e.InnerException is FileNotFoundException );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowIfPolicyNameDoesNotExist()
		 public virtual void ShouldThrowIfPolicyNameDoesNotExist()
		 {
			  // given
			  IDictionary<string, string> @params = stringMap();

			  SslPolicyConfig policyConfig = new SslPolicyConfig( "default" );

			  @params[neo4j_home.name()] = _home.AbsolutePath;
			  @params[policyConfig.BaseDirectory.name()] = "certificates/default";
			  Config config = Config.defaults( @params );

			  SslPolicyLoader sslPolicyLoader = SslPolicyLoader.Create( config, NullLogProvider.Instance );

			  // when
			  try
			  {
					sslPolicyLoader.GetPolicy( "unknown" );
					fail();
			  }
			  catch ( System.ArgumentException )
			  {
					// expected
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnNullPolicyIfNullRequested()
		 public virtual void ShouldReturnNullPolicyIfNullRequested()
		 {
			  // given
			  SslPolicyLoader sslPolicyLoader = SslPolicyLoader.Create( Config.defaults(), NullLogProvider.Instance );

			  // when
			  SslPolicy sslPolicy = sslPolicyLoader.GetPolicy( null );

			  // then
			  assertNull( sslPolicy );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowLegacyPolicyToBeConfigured()
		 public virtual void ShouldNotAllowLegacyPolicyToBeConfigured()
		 {
			  // given
			  IDictionary<string, string> @params = stringMap();

			  SslPolicyConfig policyConfig = new SslPolicyConfig( LegacySslPolicyConfig.LEGACY_POLICY_NAME );

			  @params[neo4j_home.name()] = _home.AbsolutePath;
			  @params[policyConfig.BaseDirectory.name()] = "certificates/default";
			  Config config = Config.defaults( @params );

			  try
			  {
					// when
					SslPolicyLoader.Create( config, NullLogProvider.Instance );
					fail();
			  }
			  catch ( System.ArgumentException )
			  {
					// expected
			  }
		 }
	}

}