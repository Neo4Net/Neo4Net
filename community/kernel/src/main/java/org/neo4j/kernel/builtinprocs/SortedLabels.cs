﻿/*
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
namespace Org.Neo4j.Kernel.builtinprocs
{

	using LabelSet = Org.Neo4j.@internal.Kernel.Api.LabelSet;

	public class SortedLabels
	{
		 private long[] _labels;

		 private SortedLabels( long[] labels )
		 {
			  this._labels = labels;
		 }

		 public static SortedLabels From( long[] labels )
		 {
			  Arrays.sort( labels );
			  return new SortedLabels( labels );
		 }

		 internal static SortedLabels From( LabelSet labelSet )
		 {
			  return From( labelSet.All() );
		 }

		 private long[] All()
		 {
			  return _labels;
		 }

		 public override int GetHashCode()
		 {
			  return Arrays.GetHashCode( _labels );
		 }

		 public override bool Equals( object obj )
		 {
			  if ( obj is SortedLabels )
			  {
					long[] input = ( ( SortedLabels ) obj ).All();
					return Arrays.Equals( _labels, input );
			  }
			  return false;
		 }

		 public virtual int NumberOfLabels()
		 {
			  return _labels.Length;
		 }

		 public virtual int? Label( int offset )
		 {
			  return ( int ) _labels[offset];
		 }
	}

}