using System.Collections.Concurrent;
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
namespace Neo4Net.Kernel.Internal.locker
{

	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using StoreLayout = Neo4Net.Io.layout.StoreLayout;

	/// <summary>
	/// Store locker that guarantee that only single channel ever will be opened and then closed for store locker
	/// file to prevent cases where lock will be released when any of the opened channels for file will be released as
	/// described in <seealso cref="FileLock"/> javadoc:
	/// <para>
	/// <b>
	/// On some systems, closing a channel releases all locks held by the Java virtual machine on the underlying
	/// file regardless of whether the locks were acquired via that channel or via another channel open on the same file.
	/// It is strongly recommended that, within a program, a unique channel be used to acquire all locks on any given file.
	/// </b>
	/// </para>
	/// 
	/// The guarantee is achieved by tracking all locked files over all instances of <seealso cref="GlobalStoreLocker"/>.
	/// 
	/// Class guarantee visibility of locked files over multiple thread but do not guarantee atomicity of operations.
	/// </summary>
	public class GlobalStoreLocker : StoreLocker
	{
		 private static readonly ISet<File> _lockedFiles = Collections.newSetFromMap( new ConcurrentDictionary<File>() );

		 public GlobalStoreLocker( FileSystemAbstraction fileSystemAbstraction, StoreLayout storeLayout ) : base( fileSystemAbstraction, storeLayout )
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void checkLock() throws org.Neo4Net.kernel.StoreLockException
		 public override void CheckLock()
		 {
			  base.CheckLock();
			  _lockedFiles.Add( StoreLockFile );
		 }

		 protected internal override bool HaveLockAlready()
		 {
			  if ( _lockedFiles.Contains( StoreLockFile ) )
			  {
					if ( StoreLockFileLock != null )
					{
						 return true;
					}
					throw UnableToObtainLockException();
			  }
			  return false;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void releaseLock() throws java.io.IOException
		 protected internal override void ReleaseLock()
		 {
			  _lockedFiles.remove( StoreLockFile );
			  base.ReleaseLock();
		 }
	}

}