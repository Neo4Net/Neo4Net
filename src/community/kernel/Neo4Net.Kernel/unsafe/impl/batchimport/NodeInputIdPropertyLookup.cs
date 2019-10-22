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
namespace Neo4Net.@unsafe.Impl.Batchimport
{

	using PropertyStore = Neo4Net.Kernel.impl.store.PropertyStore;
	using PropertyRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyRecord;
	using EncodingIdMapper = Neo4Net.@unsafe.Impl.Batchimport.cache.idmapping.@string.EncodingIdMapper;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.record.RecordLoad.CHECK;

	/// <summary>
	/// Looks up "input id" from a node. This is used when importing nodes and where the input data specifies ids
	/// using its own id name space, such as arbitrary strings. Those ids are called input ids and are converted
	/// into actual record ids during import. However there may be duplicate such input ids in the input data
	/// and the <seealso cref="EncodingIdMapper"/> may need to double check some input ids since it's only caching a hash
	/// of the input id in memory. The input ids are stored as properties on the nodes to be able to retrieve
	/// them for such an event. This class can look up those input id properties for arbitrary nodes.
	/// </summary>
	internal class NodeInputIdPropertyLookup : System.Func<long, object>
	{
		 private readonly PropertyStore _propertyStore;
		 private readonly PropertyRecord _propertyRecord;

		 internal NodeInputIdPropertyLookup( PropertyStore propertyStore )
		 {
			  this._propertyStore = propertyStore;
			  this._propertyRecord = propertyStore.NewRecord();
		 }

		 public override object Apply( long nodeId )
		 {
			  _propertyStore.getRecord( nodeId, _propertyRecord, CHECK );
			  if ( !_propertyRecord.inUse() )
			  {
					return null;
			  }
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  return _propertyRecord.GetEnumerator().next().newPropertyValue(_propertyStore).asObject();
		 }
	}

}