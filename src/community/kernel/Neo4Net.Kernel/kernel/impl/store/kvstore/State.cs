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
namespace Neo4Net.Kernel.impl.store.kvstore
{

	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using VersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.VersionContextSupplier;

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public class State : System.Attribute
	{
		 internal Strategy value;

		 private enum Strategy;
		 {
			  CONCURRENT_HASH_MAP
			  {
				  public <Key> ActiveState<Key> open( ReadableState<Key> store, File file, VersionContextSupplier versionContextSupplier ) { return new ConcurrentMapState<>( store, file, versionContextSupplier ); }
			  },
			  ;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: READ_ONLY_CONCURRENT_HASH_MAP { @Override public <Key> ActiveState<Key> open(ReadableState<Key> store, java.io.File file, org.Neo4Net.io.pagecache.tracing.cursor.context.VersionContextSupplier versionContextSupplier) { return new ConcurrentMapState<Key>(store, file, org.Neo4Net.io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier.EMPTY) { @Override protected boolean hasChanges() { return false; } }; } }
			  READ_ONLY_CONCURRENT_HASH_MAP
			  {
				  public <Key> ActiveState<Key> open( ReadableState<Key> store, File file, VersionContextSupplier versionContextSupplier )
				  {
					  return new ConcurrentMapState<Key>( store, file, EmptyVersionContextSupplier.EMPTY )
					  {
						  protected bool hasChanges() { return false; }
					  }; } };
				  }

		public State( Strategy value )
		{
			this.value = value;
		}
			  }

}