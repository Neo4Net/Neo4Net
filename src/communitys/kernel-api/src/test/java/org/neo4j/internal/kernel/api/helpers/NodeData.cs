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
namespace Neo4Net.@internal.Kernel.Api.helpers
{

	using Value = Neo4Net.Values.Storable.Value;

	internal class NodeData
	{
		 internal readonly long Id;
		 private readonly long[] _labels;
		 internal readonly IDictionary<int, Value> Properties;

		 internal NodeData( long id, long[] labels, IDictionary<int, Value> properties )
		 {
			  this.Id = id;
			  this._labels = labels;
			  this.Properties = properties;
		 }

		 internal virtual LabelSet LabelSet()
		 {
			  return new LabelSetAnonymousInnerClass( this );
		 }

		 private class LabelSetAnonymousInnerClass : LabelSet
		 {
			 private readonly NodeData _outerInstance;

			 public LabelSetAnonymousInnerClass( NodeData outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public int numberOfLabels()
			 {
				  return _outerInstance.labels.Length;
			 }

			 public int label( int offset )
			 {
				  return ( int ) _outerInstance.labels[offset];
			 }

			 public bool contains( int labelToken )
			 {
				  foreach ( long label in _outerInstance.labels )
				  {
						if ( label == labelToken )
						{
							 return true;
						}
				  }
				  return false;
			 }

			 public long[] all()
			 {
				  return _outerInstance.labels;
			 }
		 }
	}

}