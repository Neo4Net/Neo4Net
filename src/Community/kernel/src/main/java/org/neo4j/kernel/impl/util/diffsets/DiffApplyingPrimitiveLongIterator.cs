using System.Collections.Generic;

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
namespace Neo4Net.Kernel.impl.util.diffsets
{
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;
	using LongSet = org.eclipse.collections.api.set.primitive.LongSet;

	using PrimitiveLongBaseIterator = Neo4Net.Collection.PrimitiveLongCollections.PrimitiveLongBaseIterator;
	using PrimitiveLongResourceIterator = Neo4Net.Collection.PrimitiveLongResourceIterator;
	using Resource = Neo4Net.Graphdb.Resource;

	/// <summary>
	/// Applies a diffset to the provided <seealso cref="LongIterator"/>.
	/// </summary>
	internal class DiffApplyingPrimitiveLongIterator : PrimitiveLongBaseIterator, PrimitiveLongResourceIterator
	{
		 protected internal abstract class Phase
		 {
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           FILTERED_SOURCE { boolean fetchNext(DiffApplyingPrimitiveLongIterator self) { return self.computeNextFromSourceAndFilter(); } },

//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           ADDED_ELEMENTS { boolean fetchNext(DiffApplyingPrimitiveLongIterator self) { return self.computeNextFromAddedElements(); } },

//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           NO_ADDED_ELEMENTS { boolean fetchNext(DiffApplyingPrimitiveLongIterator self) { return false; } };

			  private static readonly IList<Phase> valueList = new List<Phase>();

			  static Phase()
			  {
				  valueList.Add( FILTERED_SOURCE );
				  valueList.Add( ADDED_ELEMENTS );
				  valueList.Add( NO_ADDED_ELEMENTS );
			  }

			  public enum InnerEnum
			  {
				  FILTERED_SOURCE,
				  ADDED_ELEMENTS,
				  NO_ADDED_ELEMENTS
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  private Phase( string name, InnerEnum innerEnum )
			  {
				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  internal abstract bool fetchNext( DiffApplyingPrimitiveLongIterator self );

			 public static IList<Phase> values()
			 {
				 return valueList;
			 }

			 public int ordinal()
			 {
				 return ordinalValue;
			 }

			 public override string ToString()
			 {
				 return nameValue;
			 }

			 public static Phase valueOf( string name )
			 {
				 foreach ( Phase enumInstance in Phase.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

		 private readonly LongIterator _source;
		 private readonly LongIterator _addedElementsIterator;
		 private readonly LongSet _addedElements;
		 private readonly LongSet _removedElements;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nullable private final org.neo4j.graphdb.Resource resource;
		 private readonly Resource _resource;
		 private Phase _phase;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: private DiffApplyingPrimitiveLongIterator(org.eclipse.collections.api.iterator.LongIterator source, org.eclipse.collections.api.set.primitive.LongSet addedElements, org.eclipse.collections.api.set.primitive.LongSet removedElements, @Nullable Resource resource)
		 private DiffApplyingPrimitiveLongIterator( LongIterator source, LongSet addedElements, LongSet removedElements, Resource resource )
		 {
			  this._source = source;
			  this._addedElements = addedElements.freeze();
			  this._addedElementsIterator = this._addedElements.longIterator();
			  this._removedElements = removedElements;
			  this._resource = resource;
			  this._phase = Phase.FilteredSource;
		 }

		 internal static LongIterator Augment( LongIterator source, LongSet addedElements, LongSet removedElements )
		 {
			  return new DiffApplyingPrimitiveLongIterator( source, addedElements, removedElements, null );
		 }

		 internal static PrimitiveLongResourceIterator Augment( PrimitiveLongResourceIterator source, LongSet addedElements, LongSet removedElements )
		 {
			  return new DiffApplyingPrimitiveLongIterator( source, addedElements, removedElements, source );
		 }

		 protected internal override bool FetchNext()
		 {
			  return _phase.fetchNext( this );
		 }

		 private bool ComputeNextFromSourceAndFilter()
		 {
			  while ( _source.hasNext() )
			  {
					long value = _source.next();
					if ( !_removedElements.contains( value ) && !_addedElements.contains( value ) )
					{
						 return Next( value );
					}
			  }
			  TransitionToAddedElements();
			  return _phase.fetchNext( this );
		 }

		 private void TransitionToAddedElements()
		 {
			  _phase = _addedElementsIterator.hasNext() ? Phase.AddedElements : Phase.NoAddedElements;
		 }

		 private bool ComputeNextFromAddedElements()
		 {
			  return _addedElementsIterator.hasNext() && Next(_addedElementsIterator.next());
		 }

		 public override void Close()
		 {
			  if ( _resource != null )
			  {
					_resource.close();
			  }
		 }
	}

}