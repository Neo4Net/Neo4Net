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
namespace Neo4Net.Io.pagecache.impl
{

	/// <summary>
	/// Thrown when a file cannot be locked in the process of opening a <seealso cref="SingleFilePageSwapper"/> for it.
	/// </summary>
	public class FileLockException : IOException
	{
		 public FileLockException( File file, OverlappingFileLockException throwable ) : base( "Already locked: " + file, throwable )
		 {
		 }

		 public FileLockException( File file ) : base( "This file is locked by another process, please ensure you don't have another Neo4Net process or tool using it: '" + file + "'.'" )
		 {
		 }
	}

}