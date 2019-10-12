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
namespace Org.Neo4j.@unsafe.Impl.Batchimport.input
{
	/// <summary>
	/// Collects items and is <seealso cref="close() closed"/> after any and all items have been collected.
	/// The <seealso cref="Collector"/> is responsible for closing whatever closeable resource received from the importer.
	/// </summary>
	public interface Collector : AutoCloseable
	{
		 void CollectBadRelationship( object startId, string startIdGroup, string type, object endId, string endIdGroup, object specificValue );

		 void CollectDuplicateNode( object id, long actualId, string group );

		 void CollectExtraColumns( string source, long row, string value );

		 long BadEntries();

		 bool CollectingBadRelationships { get; }

		 /// <summary>
		 /// Flushes whatever changes to the underlying resource supplied from the importer.
		 /// </summary>
		 void Close();

	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 Collector EMPTY = new Collector()
	//	 {
	//		  @@Override public void collectExtraColumns(String source, long row, String value)
	//		  {
	//		  }
	//
	//		  @@Override public void close()
	//		  {
	//		  }
	//
	//		  @@Override public long badEntries()
	//		  {
	//				return 0;
	//		  }
	//
	//		  @@Override public void collectBadRelationship(Object startId, String startIdGroup, String type, Object endId, String endIdGroup, Object specificValue)
	//		  {
	//		  }
	//
	//		  @@Override public void collectDuplicateNode(Object id, long actualId, String group)
	//		  {
	//		  }
	//
	//		  @@Override public boolean isCollectingBadRelationships()
	//		  {
	//				return true;
	//		  }
	//	 };
	}

}