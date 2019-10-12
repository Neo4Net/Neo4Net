using System;
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
namespace Org.Neo4j.Commandline.admin
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using BackupHelpOutput = Org.Neo4j.backup.impl.BackupHelpOutput;
	using DefaultFileSystemAbstraction = Org.Neo4j.Io.fs.DefaultFileSystemAbstraction;
	using SuppressOutput = Org.Neo4j.Test.rule.SuppressOutput;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;

	public class BackupUsageTest
	{
		 private static readonly Path _here = Paths.get( "." );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.SuppressOutput suppressOutput = org.neo4j.test.rule.SuppressOutput.suppressAll();
		 public SuppressOutput SuppressOutput = SuppressOutput.suppressAll();

		 private readonly CommandLocator _commandLocator = CommandLocator.fromServiceLocator();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void outputMatchesExpectedForMissingBackupDir()
		 public virtual void OutputMatchesExpectedForMissingBackupDir()
		 {
			  // when
			  string output = RunBackup();

			  // then
			  string reason = "Missing argument 'backup-dir'";
			  assertThat( output, containsString( reason ) );

			  // and
			  foreach ( string line in BackupHelpOutput.BACKUP_OUTPUT_LINES )
			  {
					assertThat( output, containsString( line ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void missingBackupName()
		 public virtual void MissingBackupName()
		 {
			  // when
			  string output = runBackup( "--backup-dir=target" );

			  // then
			  string reason = "Missing argument 'name'";
			  assertThat( output, containsString( reason ) );

			  // and
			  foreach ( string line in BackupHelpOutput.BACKUP_OUTPUT_LINES )
			  {
					assertThat( "Failed for line: " + line, output, containsString( line ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void incorrectBackupDirectory() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void IncorrectBackupDirectory()
		 {
			  // when
			  Path backupDirectoryResolved = _here.toRealPath().resolve("non_existing_dir");
			  string output = runBackup( "--backup-dir=non_existing_dir", "--name=mybackup" );

			  // then
			  string reason = string.Format( "command failed: Directory '{0}' does not exist.", backupDirectoryResolved );
			  assertThat( output, containsString( reason ) );
		 }

		 private string RunBackup( params string[] args )
		 {
			  return runBackup( false, args );
		 }

		 private string RunBackup( bool debug, params string[] args )
		 {
			  ParameterisedOutsideWorld outsideWorld = new ParameterisedOutsideWorld( System.console(), System.out, System.err, System.in, new DefaultFileSystemAbstraction() );
			  AdminTool subject = new AdminTool( _commandLocator, cmd => new List<>(), outsideWorld, debug );
			  Path homeDir = _here;
			  Path configDir = _here;
			  IList<string> @params = new List<object>();
			  @params.Add( "backup" );
			  ( ( IList<string> )@params ).AddRange( Arrays.asList( args ) );
			  string[] argArray = @params.ToArray();
			  subject.Execute( homeDir, configDir, argArray );

			  return SuppressOutput.ErrorVoice.ToString() + Environment.NewLine + SuppressOutput.OutputVoice.ToString();
		 }
	}

}