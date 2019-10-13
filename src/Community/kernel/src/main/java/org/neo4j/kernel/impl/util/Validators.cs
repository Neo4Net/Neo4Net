using System.Collections.Generic;

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
namespace Neo4Net.Kernel.impl.util
{

	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;

	public class Validators
	{
		 public static readonly Validator<File> RegexFileExists = file =>
		 {
		  if ( MatchingFiles( file ).Count == 0 )
		  {
				throw new System.ArgumentException( "File '" + file + "' doesn't exist" );
		  }
		 };

		 private Validators()
		 {
		 }

		 internal static IList<File> MatchingFiles( File fileWithRegexInName )
		 {
			  File parent = fileWithRegexInName.AbsoluteFile.ParentFile;
			  if ( parent == null || !parent.exists() )
			  {
					throw new System.ArgumentException( "Directory of " + fileWithRegexInName + " doesn't exist" );
			  }
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.regex.Pattern pattern = java.util.regex.Pattern.compile(fileWithRegexInName.getName());
			  Pattern pattern = Pattern.compile( fileWithRegexInName.Name );
			  IList<File> files = new List<File>();
			  foreach ( File file in parent.listFiles() )
			  {
					if ( pattern.matcher( file.Name ).matches() )
					{
						 Files.Add( file );
					}
			  }
			  return files;
		 }

		 public static readonly Validator<File> DirectoryIsWritable = value =>
		 {
		  if ( value.mkdirs() )
		  { // It's OK, we created the directory right now, which means we have write access to it
				return;
		  }

		  File test = new File( value, "_______test___" );
		  try
		  {
				test.createNewFile();
		  }
		  catch ( IOException e )
		  {
				throw new System.ArgumentException( "Directory '" + value + "' not writable: " + e.Message );
		  }
		  finally
		  {
				test.delete();
		  }
		 };

		 public static readonly Validator<File> ContainsNoExistingDatabase = value =>
		 {
		  try
		  {
			  using ( FileSystemAbstraction fileSystem = new DefaultFileSystemAbstraction() )
			  {
					if ( IsExistingDatabase( fileSystem, DatabaseLayout.of( value ) ) )
					{
						 throw new System.ArgumentException( "Directory '" + value + "' already contains a database" );
					}
			  }
		  }
		  catch ( IOException e )
		  {
				throw new UncheckedIOException( e );
		  }
		 };

		 public static readonly Validator<File> ContainsExistingDatabase = dbDir =>
		 {
		  try
		  {
			  using ( FileSystemAbstraction fileSystem = new DefaultFileSystemAbstraction() )
			  {
					if ( !IsExistingDatabase( fileSystem, DatabaseLayout.of( dbDir ) ) )
					{
						 throw new System.ArgumentException( "Directory '" + dbDir + "' does not contain a database" );
					}
			  }
		  }
		  catch ( IOException e )
		  {
				throw new UncheckedIOException( e );
		  }
		 };

		 private static bool IsExistingDatabase( FileSystemAbstraction fileSystem, DatabaseLayout layout )
		 {
			  return fileSystem.fileExists( layout.MetadataStore() );
		 }

		 public static Validator<string> InList( string[] validStrings )
		 {
			  return value =>
			  {
				if ( validStrings.noneMatch( s => s.Equals( value ) ) )
				{
					 throw new System.ArgumentException( "'" + value + "' found but must be one of: " + Arrays.ToString( validStrings ) + "." );
				}
			  };
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <T> Validator<T[]> atLeast(final String key, final int length)
		 public static Validator<T[]> AtLeast<T>( string key, int length )
		 {
			  return value =>
			  {
				if ( value.length < length )
				{
					 throw new System.ArgumentException( "Expected '" + key + "' to have at least " + length + " valid item" + ( length == 1 ? "" : "s" ) + ", but had " + value.length + " " + Arrays.ToString( value ) );
				}
			  };
		 }

		 public static Validator<T> EmptyValidator<T>()
		 {
			  return value =>
			  {
			  };
		 }

		 public static Validator<T> All<T>( params Validator<T>[] validators )
		 {
			  return value =>
			  {
				foreach ( Validator<T> validator in validators )
				{
					 validator.Validate( value );
				}
			  };
		 }
	}

}