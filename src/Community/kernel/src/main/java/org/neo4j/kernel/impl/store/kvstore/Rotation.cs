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
namespace Neo4Net.Kernel.impl.store.kvstore
{

	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public class Rotation : System.Attribute
	{
		 internal Strategy value;

		 private enum Strategy;
		 {
			  private LEFT_RIGHT
			  {
				  RotationStrategy create( FileSystemAbstraction fs, PageCache pages, ProgressiveFormat format, RotationMonitor monitor, DatabaseLayout databaseLayout ) { final File left = databaseLayout.countStoreA(); final File right = databaseLayout.countStoreB(); return new RotationStrategy.LeftRight(fs, pages, format, monitor, left, right); }
			  },
			  ;
			  private INCREMENTING
			  {
				  RotationStrategy create( FileSystemAbstraction fs, PageCache pages, ProgressiveFormat format, RotationMonitor monitor, DatabaseLayout databaseLayout ) { return new RotationStrategy.Incrementing( fs, pages, format, monitor, databaseLayout ); }
			  };

			  private abstract RotationStrategy create( FileSystemAbstraction fs, PageCache pages, ProgressiveFormat format, RotationMonitor monitor, DatabaseLayout databaseLayout );
		 }

		public Rotation( Strategy value )
		{
			this.value = value;
		}
	}

}