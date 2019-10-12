using System;
using System.Collections.Generic;
using System.IO;

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
namespace Neo4Net.Test
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using ZipUtils = Neo4Net.Io.compress.ZipUtils;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	/// <summary>
	/// A little trick to automatically tell whether or not store format was changed without
	/// incrementing the format version. This is done by keeping a zipped store file which is opened and tested on.
	/// On failure this test will fail saying that the format version needs update and also update the zipped
	/// store with the new version.
	/// </summary>
	public abstract class FormatCompatibilityVerifier
	{
		private bool InstanceFieldsInitialized = false;

		public FormatCompatibilityVerifier()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			GlobalRuleChain = RuleChain.outerRule( GlobalFs ).around( _globalDir );
		}

		 private readonly TestDirectory _globalDir = TestDirectory.testDirectory();
		 protected internal readonly DefaultFileSystemRule GlobalFs = new DefaultFileSystemRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain globalRuleChain = org.junit.rules.RuleChain.outerRule(globalFs).around(globalDir);
		 public RuleChain GlobalRuleChain;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectFormatChange() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDetectFormatChange()
		 {
			  File storeFile = _globalDir.file( StoreFileName() );
			  DoShouldDetectFormatChange( ZipName(), storeFile );
		 }

		 protected internal abstract string ZipName();

		 protected internal abstract string StoreFileName();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract void createStoreFile(java.io.File storeFile) throws java.io.IOException;
		 protected internal abstract void CreateStoreFile( File storeFile );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract void verifyFormat(java.io.File storeFile) throws java.io.IOException, FormatViolationException;
		 protected internal abstract void VerifyFormat( File storeFile );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract void verifyContent(java.io.File storeFile) throws java.io.IOException;
		 protected internal abstract void VerifyContent( File storeFile );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void doShouldDetectFormatChange(String zipName, java.io.File storeFile) throws Throwable
		 private void DoShouldDetectFormatChange( string zipName, File storeFile )
		 {
			  try
			  {
					Unzip( zipName, storeFile );
			  }
			  catch ( FileNotFoundException )
			  {
					// First time this test is run, eh?
					CreateStoreFile( storeFile );
					ZipUtils.zip( GlobalFs.get(), storeFile, _globalDir.file(zipName) );
					TellDeveloperToCommitThisFormatVersion( zipName );
			  }
			  assertTrue( zipName + " seems to be missing from resources directory", GlobalFs.get().fileExists(storeFile) );

			  // Verify format
			  try
			  {
					VerifyFormat( storeFile );
			  }
			  catch ( FormatViolationException e )
			  {
					// Good actually, or?
					assertThat( e.Message, containsString( "format version" ) );

					GlobalFs.get().deleteFile(storeFile);
					CreateStoreFile( storeFile );
					ZipUtils.zip( GlobalFs.get(), storeFile, _globalDir.file(zipName) );

					TellDeveloperToCommitThisFormatVersion( zipName );
			  }

			  // Verify content
			  try
			  {
					VerifyContent( storeFile );
			  }
			  catch ( Exception t )
			  {
					throw new AssertionError( "If this is the single failing test in this component then this failure is a strong indication that format " + "has changed without also incrementing format version(s). Please make necessary format version changes.", t );
			  }
		 }

		 private void TellDeveloperToCommitThisFormatVersion( string zipName )
		 {
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: fail(String.format("This is merely a notification to developer. Format has changed and its version has also " + "been properly incremented. A store file with this new format has been generated and should be committed. " + "Please move:%n  %s%ninto %n  %s, %nreplacing the existing file there", globalDir.file(zipName), "<corresponding-module>" + pathify(".src.test.resources.") + pathify(getClass().getPackage().getName() + ".") + zipName));
			  fail( string.Format( "This is merely a notification to developer. Format has changed and its version has also " + "been properly incremented. A store file with this new format has been generated and should be committed. " + "Please move:%n  %s%ninto %n  %s, %nreplacing the existing file there", _globalDir.file( zipName ), "<corresponding-module>" + Pathify( ".src.test.resources." ) + Pathify( this.GetType().Assembly.GetName().Name + "." ) + zipName ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void unzip(String zipName, java.io.File storeFile) throws java.io.IOException
		 private void Unzip( string zipName, File storeFile )
		 {
			  URL resource = this.GetType().getResource(zipName);
			  if ( resource == null )
			  {
					throw new FileNotFoundException();
			  }

			  using ( ZipFile zipFile = new ZipFile( resource.File ) )
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Iterator<? extends java.util.zip.ZipEntry> entries = zipFile.entries();
					IEnumerator<ZipEntry> entries = zipFile.entries();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( entries.hasMoreElements() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					ZipEntry entry = entries.nextElement();
					assertEquals( storeFile.Name, entry.Name );
					Files.copy( zipFile.getInputStream( entry ), storeFile.toPath() );
			  }
		 }

		 private static string Pathify( string name )
		 {
			  return name.Replace( '.', Path.DirectorySeparatorChar );
		 }

		 public class FormatViolationException : Exception
		 {
			 private readonly FormatCompatibilityVerifier _outerInstance;

			  public FormatViolationException( FormatCompatibilityVerifier outerInstance, Exception cause ) : base( cause )
			  {
				  this._outerInstance = outerInstance;
			  }

			  public FormatViolationException( FormatCompatibilityVerifier outerInstance, string message ) : base( message )
			  {
				  this._outerInstance = outerInstance;
			  }
		 }
	}

}