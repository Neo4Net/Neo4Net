﻿/*
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
namespace Neo4Net.Graphdb.mockfs
{
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;

	/// <summary>
	/// File system abstraction that wraps real file system and prevent it from being closed.
	/// Useful for cases when we pass file system to db, shut it down and verify file system content.
	/// </summary>
	public class UncloseableDelegatingFileSystemAbstraction : DelegatingFileSystemAbstraction
	{
		 public UncloseableDelegatingFileSystemAbstraction( FileSystemAbstraction @delegate ) : base( @delegate )
		 {
		 }

		 public override void Close()
		 {
			  // do nothing
		 }
	}

}