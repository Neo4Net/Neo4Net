using System.Collections.Generic;

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
namespace Neo4Net.com.storecopy
{

	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;

	public class FileMoveProvider
	{
		 private readonly FileSystemAbstraction _fs;

		 public FileMoveProvider( FileSystemAbstraction fs )
		 {
			  this._fs = fs;
		 }

		 /// <summary>
		 /// Construct a stream of files that are to be moved
		 /// </summary>
		 /// <param name="dir"> the source location of the move action </param>
		 /// <returns> a stream of the entire contents of the source location that can be applied to a target location to
		 /// perform a move </returns>
		 public virtual Stream<FileMoveAction> TraverseForMoving( File dir )
		 {
			  return TraverseForMoving( dir, dir );
		 }

		 /// <summary>
		 /// Copies <b>the contents</b> from the directory to the base target path.
		 /// <para>
		 /// This is confusing, so here is an example
		 /// </para>
		 /// <para>
		 /// </para>
		 /// <para>
		 /// <code>
		 /// +Parent<br>
		 /// |+--directoryA<br>
		 /// |...+--fileA<br>
		 /// |...+--fileB<br>
		 /// </code>
		 /// </para>
		 /// <para>
		 /// Suppose we want to move to move <b>Parent/directoryA</b> to <b>Parent/directoryB</b>.<br>
		 /// </para>
		 /// <para>
		 /// <code>
		 /// File directoryA = new File("Parent/directoryA");<br>
		 /// Stream<FileMoveAction> fileMoveActions = new FileMoveProvider(pageCache).traverseGenerateMoveActions
		 /// (directoryA, directoryA);<br>
		 /// </code>
		 /// </para>
		 /// In the above we clearly generate actions for moving all the files contained in directoryA. directoryA is
		 /// mentioned twice due to a implementation detail,
		 /// hence the public method with only one parameter. We then actually perform the moves by applying the base
		 /// target directory that we want to move to.
		 /// <para>
		 /// <code>
		 /// File directoryB = new File("Parent/directoryB");<br>
		 /// fileMoveActions.forEach( action -> action.move( directoryB ) );
		 /// </code>
		 /// </para>
		 /// </summary>
		 /// <param name="dir"> this directory and all the child paths under it are subject to move </param>
		 /// <param name="basePath"> this is the parent of your intended target directory. </param>
		 /// <returns> a stream of individual move actions which can be iterated and applied whenever </returns>
		 private Stream<FileMoveAction> TraverseForMoving( File dir, File basePath )
		 {
			  // Note that flatMap is an *intermediate operation* and therefor always lazy.
			  // It is very important that the stream we return only *lazily* calls out to expandTraverseFiles!
			  return Stream.of( dir ).flatMap( d => ExpandTraverseFiles( d, basePath ) );
		 }

		 private Stream<FileMoveAction> ExpandTraverseFiles( File dir, File basePath )
		 {
			  IList<File> listing = ListFiles( dir );
			  if ( listing == null )
			  {
					// This happens if what we were given as 'dir' is not actually a directory, but a single specific file.
					// In that case, we will produce a stream of a single FileMoveAction for that file.
					listing = Collections.singletonList( dir );
					// This also means that the base path is currently the same as the file itself, which is wrong.
					// We change the base path to be the parent directory of the file, so that we can relativise the filename
					// correctly later.
					basePath = dir.ParentFile;
			  }
			  File @base = basePath; // Capture effectively-final base path snapshot.
			  Stream<File> files = listing.Where( this.isFile );
			  Stream<File> dirs = listing.Where( this.isDirectory );
			  Stream<FileMoveAction> moveFiles = Files.map( f => MoveFileCorrectly( f, @base ) );
			  Stream<FileMoveAction> traverseDirectories = dirs.flatMap( d => TraverseForMoving( d, @base ) );
			  return Stream.concat( moveFiles, traverseDirectories );
		 }

		 private bool IsFile( File file )
		 {
			  return !_fs.isDirectory( file );
		 }

		 private bool IsDirectory( File file )
		 {
			  return _fs.isDirectory( file );
		 }

		 private IList<File> ListFiles( File dir )
		 {
			  File[] fsaFiles = _fs.listFiles( dir );
			  if ( fsaFiles == null )
			  {
					// This probably means 'dir' is actually a file, or it does not exist.
					return null;
			  }

			  return java.util.fsaFiles.Distinct().ToList();
		 }

		 /// <summary>
		 /// Some files are handled via page cache for CAPI flash, others are only used on the default file system. This
		 /// contains the logic for handling files between the 2 systems
		 /// </summary>
		 private FileMoveAction MoveFileCorrectly( File fileToMove, File basePath )
		 {
			  return FileMoveAction.moveViaFileSystem( fileToMove, basePath );
		 }
	}

}