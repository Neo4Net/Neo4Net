using System;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Kernel.impl.pagecache
{

	using IOUtils = Neo4Net.Io.IOUtils;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PagedFile = Neo4Net.Io.pagecache.PagedFile;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.pagecache.PageCacheWarmer.SUFFIX_CACHEPROF;

	internal sealed class Profile : IComparable<Profile>
	{
		 private const string PROFILE_DIR = "profiles";
		 private readonly File _profileFile;
		 private readonly File _pagedFile;
		 private readonly long _profileSequenceId;

		 private Profile( File profileFile, File pagedFile, long profileSequenceId )
		 {
			  Objects.requireNonNull( profileFile );
			  Objects.requireNonNull( pagedFile );
			  this._profileFile = profileFile;
			  this._pagedFile = pagedFile;
			  this._profileSequenceId = profileSequenceId;
		 }

		 public override int CompareTo( Profile that )
		 {
			  int compare = _pagedFile.compareTo( that._pagedFile );
			  return compare == 0 ? Long.compare( _profileSequenceId, that._profileSequenceId ) : compare;
		 }

		 public override bool Equals( object o )
		 {
			  if ( o is Profile )
			  {
					Profile profile = ( Profile ) o;
					return _profileFile.Equals( profile._profileFile );
			  }
			  return false;
		 }

		 public override int GetHashCode()
		 {
			  return _profileFile.GetHashCode();
		 }

		 public override string ToString()
		 {
			  return "Profile(" + _profileSequenceId + " for " + _pagedFile + ")";
		 }

		 internal File File()
		 {
			  return _profileFile;
		 }

		 internal void Delete( FileSystemAbstraction fs )
		 {
			  fs.DeleteFile( _profileFile );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.io.InputStream read(org.Neo4Net.io.fs.FileSystemAbstraction fs) throws java.io.IOException
		 internal Stream Read( FileSystemAbstraction fs )
		 {
			  Stream source = fs.OpenAsInputStream( _profileFile );
			  try
			  {
					return new GZIPInputStream( source );
			  }
			  catch ( IOException e )
			  {
					IOUtils.closeAllSilently( source );
					throw new IOException( "Exception when building decompressor.", e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.io.OutputStream write(org.Neo4Net.io.fs.FileSystemAbstraction fs) throws java.io.IOException
		 internal Stream Write( FileSystemAbstraction fs )
		 {
			  fs.Mkdirs( _profileFile.ParentFile ); // Create PROFILE_FOLDER if it does not exist.
			  Stream sink = fs.OpenAsOutputStream( _profileFile, false );
			  try
			  {
					return new GZIPOutputStream( sink );
			  }
			  catch ( IOException e )
			  {
					IOUtils.closeAllSilently( sink );
					throw new IOException( "Exception when building compressor.", e );
			  }
		 }

		 internal Profile Next()
		 {
			  long next = _profileSequenceId + 1L;
			  return new Profile( ProfileName( _pagedFile, next ), _pagedFile, next );
		 }

		 internal static Profile First( File file )
		 {
			  return new Profile( ProfileName( file, 0 ), file, 0 );
		 }

		 private static File ProfileName( File file, long count )
		 {
			  string name = file.Name;
			  File dir = new File( file.ParentFile, PROFILE_DIR );
			  return new File( dir, name + "." + Convert.ToString( count ) + SUFFIX_CACHEPROF );
		 }

		 internal static System.Predicate<Profile> RelevantTo( PagedFile pagedFile )
		 {
			  return p => p.pagedFile.Equals( pagedFile.File() );
		 }

		 internal static Stream<Profile> FindProfilesInDirectory( FileSystemAbstraction fs, File dir )
		 {
			  File[] files = fs.ListFiles( new File( dir, PROFILE_DIR ) );
			  if ( files == null )
			  {
					return Stream.empty();
			  }
			  return Stream.of( files ).flatMap( Profile.parseProfileName );
		 }

		 private static Stream<Profile> ParseProfileName( File profile )
		 {
			  File profileFolder = profile.ParentFile;
			  File dir = profileFolder.ParentFile;
			  string name = profile.Name;
			  if ( !name.EndsWith( SUFFIX_CACHEPROF, StringComparison.Ordinal ) )
			  {
					return Stream.empty();
			  }
			  int lastDot = name.LastIndexOf( '.' );
			  int secondLastDot = name.LastIndexOf( '.', lastDot - 1 );
			  string countStr = name.Substring( secondLastDot + 1, lastDot - ( secondLastDot + 1 ) );
			  try
			  {
					long sequenceId = Convert.ToInt64( countStr, 10 );
					string mappedFileName = name.Substring( 0, secondLastDot );
					return Stream.of( new Profile( profile, new File( dir, mappedFileName ), sequenceId ) );
			  }
			  catch ( System.FormatException )
			  {
					return Stream.empty();
			  }
		 }
	}

}