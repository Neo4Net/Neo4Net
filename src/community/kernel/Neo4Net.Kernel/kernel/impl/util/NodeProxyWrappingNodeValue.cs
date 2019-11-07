﻿using System;
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
namespace Neo4Net.Kernel.impl.util
{

	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using NotFoundException = Neo4Net.GraphDb.NotFoundException;
	using NodeProxy = Neo4Net.Kernel.impl.core.NodeProxy;
	using InvalidRecordException = Neo4Net.Kernel.impl.store.InvalidRecordException;
	using Neo4Net.Values;
	using TextArray = Neo4Net.Values.Storable.TextArray;
	using Values = Neo4Net.Values.Storable.Values;
	using MapValue = Neo4Net.Values.@virtual.MapValue;
	using NodeValue = Neo4Net.Values.@virtual.NodeValue;
	using VirtualValues = Neo4Net.Values.@virtual.VirtualValues;

	public class NodeProxyWrappingNodeValue : NodeValue
	{
		 private readonly Node _node;
		 private volatile TextArray _labels;
		 private volatile MapValue _properties;

		 internal NodeProxyWrappingNodeValue( Node node ) : base( node.Id )
		 {
			  this._node = node;
		 }

		 public virtual Node NodeProxy()
		 {
			  return _node;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <E extends Exception> void WriteTo(Neo4Net.values.AnyValueWriter<E> writer) throws E
		 public override void WriteTo<E>( AnyValueWriter<E> writer ) where E : Exception
		 {
			  TextArray l;
			  MapValue p;
			  try
			  {
					l = Labels();
					p = Properties();
			  }
			  catch ( NotFoundException )
			  {
					l = Values.stringArray();
					p = VirtualValues.EMPTY_MAP;
			  }
			  catch ( InvalidRecordException e )
			  {
					throw new ReadAndDeleteTransactionConflictException( NodeProxy.isDeletedInCurrentTransaction( _node ), e );
			  }

			  if ( Id() < 0 )
			  {
					writer.WriteVirtualNodeHack( _node );
			  }

			  writer.WriteNode( _node.Id, l, p );
		 }

		 public override TextArray Labels()
		 {
			  TextArray l = _labels;
			  if ( l == null )
			  {
					lock ( this )
					{
						 l = _labels;
						 if ( l == null )
						 {
							  List<string> ls = new List<string>();
							  foreach ( Label label in _node.Labels )
							  {
									ls.Add( label.Name() );
							  }
							  l = _labels = Values.stringArray( ls.ToArray() );

						 }
					}
			  }
			  return l;
		 }

		 public override MapValue Properties()
		 {
			  MapValue m = _properties;
			  if ( m == null )
			  {
					lock ( this )
					{
						 m = _properties;
						 if ( m == null )
						 {
							  m = _properties = ValueUtils.AsMapValue( _node.AllProperties );
						 }
					}
			  }
			  return m;
		 }
	}

}