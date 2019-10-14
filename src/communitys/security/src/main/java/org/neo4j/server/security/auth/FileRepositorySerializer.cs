using System;
using System.Collections.Generic;
using System.Text;

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
namespace Neo4Net.Server.Security.Auth
{

	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using FormatException = Neo4Net.Server.Security.Auth.exception.FormatException;
	using UTF8 = Neo4Net.Strings.UTF8;


	public abstract class FileRepositorySerializer<S>
	{
		 private Random _random = new SecureRandom();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void writeToFile(org.neo4j.io.fs.FileSystemAbstraction fs, java.io.File file, byte[] bytes) throws java.io.IOException
		 public static void WriteToFile( FileSystemAbstraction fs, File file, sbyte[] bytes )
		 {
			  using ( Stream o = fs.OpenAsOutputStream( file, false ) )
			  {
					o.Write( bytes, 0, bytes.Length );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static java.util.List<String> readFromFile(org.neo4j.io.fs.FileSystemAbstraction fs, java.io.File file) throws java.io.IOException
		 public static IList<string> ReadFromFile( FileSystemAbstraction fs, File file )
		 {
			  List<string> lines = new List<string>();

			  using ( StreamReader r = new StreamReader( fs.OpenAsReader( file, UTF_8 ) ) )
			  {
					while ( true )
					{
						 string line = r.ReadLine();
						 if ( string.ReferenceEquals( line, null ) )
						 {
							  break;
						 }
						 lines.Add( line );
					}
			  }

			  return lines;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void saveRecordsToFile(org.neo4j.io.fs.FileSystemAbstraction fileSystem, java.io.File recordsFile, java.util.Collection<S> records) throws java.io.IOException
		 public virtual void SaveRecordsToFile( FileSystemAbstraction fileSystem, File recordsFile, ICollection<S> records )
		 {
			  File tempFile = GetTempFile( fileSystem, recordsFile );

			  try
			  {
					WriteToFile( fileSystem, tempFile, Serialize( records ) );
					fileSystem.RenameFile( tempFile, recordsFile, ATOMIC_MOVE, REPLACE_EXISTING );
			  }
			  catch ( Exception e )
			  {
					fileSystem.DeleteFile( tempFile );
					throw e;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected java.io.File getTempFile(org.neo4j.io.fs.FileSystemAbstraction fileSystem, java.io.File recordsFile) throws java.io.IOException
		 protected internal virtual File GetTempFile( FileSystemAbstraction fileSystem, File recordsFile )
		 {
			  File directory = recordsFile.ParentFile;
			  if ( !fileSystem.FileExists( directory ) )
			  {
					fileSystem.Mkdirs( directory );
			  }

			  long n = _random.nextLong();
			  n = ( n == long.MinValue ) ? 0 : Math.Abs( n );
			  return new File( directory, Convert.ToString( n ) + "_" + recordsFile.Name + ".tmp" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.List<S> loadRecordsFromFile(org.neo4j.io.fs.FileSystemAbstraction fileSystem, java.io.File recordsFile) throws java.io.IOException, org.neo4j.server.security.auth.exception.FormatException
		 public virtual IList<S> LoadRecordsFromFile( FileSystemAbstraction fileSystem, File recordsFile )
		 {
			  return DeserializeRecords( ReadFromFile( fileSystem, recordsFile ) );
		 }

		 public virtual sbyte[] Serialize( ICollection<S> records )
		 {
			  StringBuilder sb = new StringBuilder();
			  foreach ( S record in records )
			  {
					sb.Append( Serialize( record ) ).Append( "\n" );
			  }
			  return UTF8.encode( sb.ToString() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.List<S> deserializeRecords(byte[] bytes) throws org.neo4j.server.security.auth.exception.FormatException
		 public virtual IList<S> DeserializeRecords( sbyte[] bytes )
		 {
			  return DeserializeRecords( Arrays.asList( UTF8.decode( bytes ).Split( "\n", true ) ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.List<S> deserializeRecords(java.util.List<String> lines) throws org.neo4j.server.security.auth.exception.FormatException
		 public virtual IList<S> DeserializeRecords( IList<string> lines )
		 {
			  IList<S> @out = new List<S>();
			  int lineNumber = 1;
			  foreach ( string line in lines )
			  {
					if ( line.Trim().Length > 0 )
					{
						 @out.Add( DeserializeRecord( line, lineNumber ) );
					}
					lineNumber++;
			  }
			  return @out;
		 }

		 protected internal abstract string Serialize( S record );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract S deserializeRecord(String line, int lineNumber) throws org.neo4j.server.security.auth.exception.FormatException;
		 protected internal abstract S DeserializeRecord( string line, int lineNumber );
	}

}