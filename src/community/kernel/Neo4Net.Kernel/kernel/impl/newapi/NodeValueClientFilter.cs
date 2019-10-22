using System;

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
namespace Neo4Net.Kernel.Impl.Newapi
{
	using IndexOrder = Neo4Net.Internal.Kernel.Api.IndexOrder;
	using IndexQuery = Neo4Net.Internal.Kernel.Api.IndexQuery;
	using NodeCursor = Neo4Net.Internal.Kernel.Api.NodeCursor;
	using PropertyCursor = Neo4Net.Internal.Kernel.Api.PropertyCursor;
	using Read = Neo4Net.Internal.Kernel.Api.Read;
	using IOUtils = Neo4Net.Io.IOUtils;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;
	using IndexProgressor = Neo4Net.Storageengine.Api.schema.IndexProgressor;
	using IndexProgressor_NodeValueClient = Neo4Net.Storageengine.Api.schema.IndexProgressor_NodeValueClient;
	using Value = Neo4Net.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.NO_VALUE;

	/// <summary>
	/// This class filters acceptNode() calls from an index progressor, to assert that exact entries returned from the
	/// progressor really match the exact property values. See also org.Neo4Net.kernel.impl.api.LookupFilter.
	/// <para>
	/// It works by acting as a man-in-the-middle between outer <seealso cref="NodeValueClient client"/> and inner <seealso cref="IndexProgressor"/>.
	/// Interaction goes like:
	/// </para>
	/// <para>
	/// Initialize:
	/// <pre><code>
	/// client
	///      -- query( client ) ->      filter = new filter(client)
	///                                 filter -- query( filter ) ->        progressor
	///                                 filter <- initialize(progressor) -- progressor
	/// client <- initialize(filter) -- filter
	/// </code></pre>
	/// </para>
	/// <para>
	/// Progress:
	/// <pre><code>
	/// client -- next() ->       filter
	///                           filter -- next() ->          progressor
	///                                     <- acceptNode() --
	///                                  -- :false ->
	///                                     <- acceptNode() --
	///                                  -- :false ->
	///                           filter    <- acceptNode() --
	/// client <- acceptNode() -- filter
	///        -- :true ->        filter -- :true ->           progressor
	/// client <----------------------------------------------
	/// </code></pre>
	/// </para>
	/// <para>
	/// Close:
	/// <pre><code>
	/// client -- close() -> filter
	///                      filter -- close() -> progressor
	/// client <---------------------------------
	/// </code></pre>
	/// </para>
	/// </summary>
	internal class NodeValueClientFilter : IndexProgressor_NodeValueClient, IndexProgressor
	{
		 private readonly IndexProgressor_NodeValueClient _target;
		 private readonly NodeCursor _node;
		 private readonly PropertyCursor _property;
		 private readonly IndexQuery[] _filters;
		 private readonly Read _read;
		 private IndexProgressor _progressor;

		 internal NodeValueClientFilter( IndexProgressor_NodeValueClient target, NodeCursor node, PropertyCursor property, Read read, params IndexQuery[] filters )
		 {
			  this._target = target;
			  this._node = node;
			  this._property = property;
			  this._filters = filters;
			  this._read = read;
		 }

		 public override void Initialize( IndexDescriptor descriptor, IndexProgressor progressor, IndexQuery[] query, IndexOrder indexOrder, bool needsValues )
		 {
			  this._progressor = progressor;
			  _target.initialize( descriptor, this, query, indexOrder, needsValues );
		 }

		 public override bool AcceptNode( long reference, Value[] values )
		 {
			  // First filter on these values, which come from the index. Some values will be NO_VALUE, because some indexed values cannot be read back.
			  // Those values will have to be read from the store using the propertyCursor and is done in one pass after this loop, if needed.
			  int storeLookups = 0;
			  if ( values == null )
			  {
					// values == null effectively means that all values are NO_VALUE so we certainly need the store lookup here
					foreach ( IndexQuery filter in _filters )
					{
						 if ( filter != null )
						 {
							  storeLookups++;
						 }
					}
			  }
			  else
			  {
					for ( int i = 0; i < _filters.Length; i++ )
					{
						 IndexQuery filter = _filters[i];
						 if ( filter != null )
						 {
							  if ( values[i] == NO_VALUE )
							  {
									storeLookups++;
							  }
							  else if ( !filter.AcceptsValue( values[i] ) )
							  {
									return false;
							  }
						 }
					}
			  }

			  // If there were one or more NO_VALUE values above then open store cursor and read those values from the store,
			  // applying the same filtering as above, but with a loop designed to do only a single pass over the store values,
			  // because it's the most expensive part.
			  if ( storeLookups > 0 && !AcceptByStoreFiltering( reference, storeLookups, values ) )
			  {
					return false;
			  }
			  return _target.acceptNode( reference, values );
		 }

		 private bool AcceptByStoreFiltering( long reference, int storeLookups, Value[] values )
		 {
			  // Initialize the property cursor scan
			  _read.singleNode( reference, _node );
			  if ( !_node.next() )
			  {
					// This node doesn't exist, therefore it cannot be accepted
					_property.close();
					return false;
			  }
			  _node.properties( _property );

			  while ( storeLookups > 0 && _property.next() )
			  {
					for ( int i = 0; i < _filters.Length; i++ )
					{
						 IndexQuery filter = _filters[i];
						 if ( filter != null && ( values == null || values[i] == NO_VALUE ) && _property.propertyKey() == filter.PropertyKeyId() )
						 {
							  if ( !filter.AcceptsValueAt( _property ) )
							  {
									return false;
							  }
							  storeLookups--;
						 }
					}
			  }
			  return storeLookups == 0;
		 }

		 public override bool NeedsValues()
		 {
			  // We return needsValues = true to the progressor, since this will enable us to execute the cheaper filterByIndexValues
			  // instead of filterByCursors if the progressor can provide values.
			  return true;
		 }

		 public override bool Next()
		 {
			  return _progressor.next();
		 }

		 public override void Close()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  IOUtils.close( Exception::new, _node, _property, _progressor );
		 }
	}

}