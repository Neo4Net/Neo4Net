/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.Ssl
{
	using SystemUtils = org.apache.commons.lang.SystemUtils;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assume.assumeTrue;

	public class SelfSignedCertificatesIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createSelfSignedCertificateWithCorrectPermissions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CreateSelfSignedCertificateWithCorrectPermissions()
		 {
			  assumeTrue( !SystemUtils.IS_OS_WINDOWS );

			  PkiUtils certificates = new PkiUtils();
			  certificates.CreateSelfSignedCertificate( TestDirectory.file( "certificate" ), TestDirectory.file( "privateKey" ), "localhost" );

			  PosixFileAttributes certificateAttributes = Files.getFileAttributeView( TestDirectory.file( "certificate" ).toPath(), typeof(PosixFileAttributeView) ).readAttributes();

			  assertTrue( certificateAttributes.permissions().contains(PosixFilePermission.OWNER_READ) );
			  assertTrue( certificateAttributes.permissions().contains(PosixFilePermission.OWNER_WRITE) );
			  assertFalse( certificateAttributes.permissions().contains(PosixFilePermission.OWNER_EXECUTE) );

			  assertFalse( certificateAttributes.permissions().contains(PosixFilePermission.GROUP_READ) );
			  assertFalse( certificateAttributes.permissions().contains(PosixFilePermission.GROUP_WRITE) );
			  assertFalse( certificateAttributes.permissions().contains(PosixFilePermission.GROUP_EXECUTE) );

			  assertFalse( certificateAttributes.permissions().contains(PosixFilePermission.OTHERS_READ) );
			  assertFalse( certificateAttributes.permissions().contains(PosixFilePermission.OTHERS_WRITE) );
			  assertFalse( certificateAttributes.permissions().contains(PosixFilePermission.OTHERS_EXECUTE) );

			  PosixFileAttributes privateKey = Files.getFileAttributeView( TestDirectory.file( "privateKey" ).toPath(), typeof(PosixFileAttributeView) ).readAttributes();

			  assertTrue( privateKey.permissions().contains(PosixFilePermission.OWNER_READ) );
			  assertTrue( privateKey.permissions().contains(PosixFilePermission.OWNER_WRITE) );
			  assertFalse( privateKey.permissions().contains(PosixFilePermission.OWNER_EXECUTE) );

			  assertFalse( privateKey.permissions().contains(PosixFilePermission.GROUP_READ) );
			  assertFalse( privateKey.permissions().contains(PosixFilePermission.GROUP_WRITE) );
			  assertFalse( privateKey.permissions().contains(PosixFilePermission.GROUP_EXECUTE) );

			  assertFalse( privateKey.permissions().contains(PosixFilePermission.OTHERS_READ) );
			  assertFalse( privateKey.permissions().contains(PosixFilePermission.OTHERS_WRITE) );
			  assertFalse( privateKey.permissions().contains(PosixFilePermission.OTHERS_EXECUTE) );
		 }
	}

}