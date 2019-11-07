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
namespace Neo4Net.Kernel.Impl.Newapi
{
	using LongIterable = org.eclipse.collections.api.LongIterable;
	using MutableList = org.eclipse.collections.api.list.MutableList;
	using MutableLongList = org.eclipse.collections.api.list.primitive.MutableLongList;
	using LongSet = org.eclipse.collections.api.set.primitive.LongSet;
	using MutableLongSet = org.eclipse.collections.api.set.primitive.MutableLongSet;
	using UnmodifiableMap = org.eclipse.collections.impl.UnmodifiableMap;
	using Lists = org.eclipse.collections.impl.factory.Lists;
	using LongLists = org.eclipse.collections.impl.factory.primitive.LongLists;
	using LongSets = org.eclipse.collections.impl.factory.primitive.LongSets;


	using IndexOrder = Neo4Net.Kernel.Api.Internal.IndexOrder;
	using IndexQuery = Neo4Net.Kernel.Api.Internal.IndexQuery;
	using IndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor;
	using LongDiffSets = Neo4Net.Kernel.Api.StorageEngine.TxState.LongDiffSets;
	using ReadableTransactionState = Neo4Net.Kernel.Api.StorageEngine.TxState.ReadableTransactionState;
	using TextValue = Neo4Net.Values.Storable.TextValue;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueTuple = Neo4Net.Values.Storable.ValueTuple;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.NO_VALUE;

	/// <summary>
	/// This class provides static utility methods that calculate relevant index updates from a transaction state for several index operations.
	/// </summary>
	internal class TxStateIndexChanges
	{

		 private static readonly AddedWithValuesAndRemoved _emptyAddedAndRemovedWithValues = new AddedWithValuesAndRemoved( Collections.emptyList(), LongSets.immutable.empty() );
		 private static readonly AddedAndRemoved _emptyAddedAndRemoved = new AddedAndRemoved( LongLists.immutable.empty(), LongSets.immutable.empty() );
		 private static readonly ValueTuple _maxStringTuple = ValueTuple.of( Values.MAX_STRING );

		 // SCAN

		 internal static AddedAndRemoved IndexUpdatesForScan( ReadableTransactionState txState, IndexDescriptor descriptor, IndexOrder indexOrder )
		 {
			  return IndexUpdatesForScanAndFilter( txState, descriptor, null, indexOrder );
		 }

		 internal static AddedWithValuesAndRemoved IndexUpdatesWithValuesForScan( ReadableTransactionState txState, IndexDescriptor descriptor, IndexOrder indexOrder )
		 {
			  return IndexUpdatesWithValuesScanAndFilter( txState, descriptor, null, indexOrder );
		 }

		 // SUFFIX

		 internal static AddedAndRemoved IndexUpdatesForSuffixOrContains( ReadableTransactionState txState, IndexDescriptor descriptor, IndexQuery query, IndexOrder indexOrder )
		 {
			  if ( descriptor.Schema().PropertyIds.Length != 1 )
			  {
					throw new System.InvalidOperationException( "Suffix and contains queries are only supported for single property queries" );
			  }
			  return IndexUpdatesForScanAndFilter( txState, descriptor, query, indexOrder );
		 }

		 internal static AddedWithValuesAndRemoved IndexUpdatesWithValuesForSuffixOrContains( ReadableTransactionState txState, IndexDescriptor descriptor, IndexQuery query, IndexOrder indexOrder )
		 {
			  if ( descriptor.Schema().PropertyIds.Length != 1 )
			  {
					throw new System.InvalidOperationException( "Suffix and contains queries are only supported for single property queries" );
			  }
			  return IndexUpdatesWithValuesScanAndFilter( txState, descriptor, query, indexOrder );
		 }

		 // SEEK

		 internal static AddedAndRemoved IndexUpdatesForSeek( ReadableTransactionState txState, IndexDescriptor descriptor, ValueTuple values )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.eclipse.collections.impl.UnmodifiableMap<Neo4Net.values.storable.ValueTuple,? extends Neo4Net.Kernel.Api.StorageEngine.TxState.LongDiffSets> updates = txState.getIndexUpdates(descriptor.schema());
			  UnmodifiableMap<ValueTuple, ? extends LongDiffSets> updates = txState.GetIndexUpdates( descriptor.Schema() );
			  if ( updates != null )
			  {
					LongDiffSets indexUpdatesForSeek = updates.get( values );
					return indexUpdatesForSeek == null ? _emptyAddedAndRemoved : new AddedAndRemoved( LongLists.mutable.ofAll( indexUpdatesForSeek.Added ), indexUpdatesForSeek.Removed );
			  }
			  return _emptyAddedAndRemoved;
		 }

		 // RANGE SEEK

		 internal static AddedAndRemoved IndexUpdatesForRangeSeek<T1>( ReadableTransactionState txState, IndexDescriptor descriptor, IndexQuery.RangePredicate<T1> predicate, IndexOrder indexOrder )
		 {
			  Value lower = predicate.FromValue();
			  Value upper = predicate.ToValue();
			  if ( lower == null || upper == null )
			  {
					throw new System.InvalidOperationException( "Use Values.NO_VALUE to encode the lack of a bound" );
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.NavigableMap<Neo4Net.values.storable.ValueTuple,? extends Neo4Net.Kernel.Api.StorageEngine.TxState.LongDiffSets> sortedUpdates = txState.getSortedIndexUpdates(descriptor.schema());
			  NavigableMap<ValueTuple, ? extends LongDiffSets> sortedUpdates = txState.GetSortedIndexUpdates( descriptor.Schema() );
			  if ( sortedUpdates == null )
			  {
					return _emptyAddedAndRemoved;
			  }

			  ValueTuple selectedLower;
			  bool selectedIncludeLower;

			  ValueTuple selectedUpper;
			  bool selectedIncludeUpper;

			  if ( lower == NO_VALUE )
			  {
					selectedLower = ValueTuple.of( Values.minValue( predicate.ValueGroup(), upper ) );
					selectedIncludeLower = true;
			  }
			  else
			  {
					selectedLower = ValueTuple.of( lower );
					selectedIncludeLower = predicate.FromInclusive();
			  }

			  if ( upper == NO_VALUE )
			  {
					selectedUpper = ValueTuple.of( Values.maxValue( predicate.ValueGroup(), lower ) );
					selectedIncludeUpper = false;
			  }
			  else
			  {
					selectedUpper = ValueTuple.of( upper );
					selectedIncludeUpper = predicate.ToInclusive();
			  }

			  MutableLongList added = LongLists.mutable.empty();
			  MutableLongSet removed = LongSets.mutable.empty();

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Map<Neo4Net.values.storable.ValueTuple,? extends Neo4Net.Kernel.Api.StorageEngine.TxState.LongDiffSets> inRange = sortedUpdates.subMap(selectedLower, selectedIncludeLower, selectedUpper, selectedIncludeUpper);
			  IDictionary<ValueTuple, ? extends LongDiffSets> inRange = sortedUpdates.subMap( selectedLower, selectedIncludeLower, selectedUpper, selectedIncludeUpper );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (java.util.Map.Entry<Neo4Net.values.storable.ValueTuple,? extends Neo4Net.Kernel.Api.StorageEngine.TxState.LongDiffSets> entry : inRange.entrySet())
			  foreach ( KeyValuePair<ValueTuple, ? extends LongDiffSets> entry in inRange.SetOfKeyValuePairs() )
			  {
					ValueTuple values = entry.Key;
					LongDiffSets diffForSpecificValue = entry.Value;

					// The TreeMap cannot perfectly order multi-dimensional types (spatial) and need additional filtering out false positives
					// TODO: If the composite index starts to be able to handle spatial types the line below needs enhancement
					if ( predicate.RegularOrder || predicate.AcceptsValue( values.OnlyValue ) )
					{
						 added.addAll( diffForSpecificValue.Added );
						 removed.addAll( diffForSpecificValue.Removed );
					}
			  }
			  return new AddedAndRemoved( indexOrder == IndexOrder.DESCENDING ? added.asReversed() : added, removed );
		 }

		 internal static AddedWithValuesAndRemoved IndexUpdatesWithValuesForRangeSeek<T1>( ReadableTransactionState txState, IndexDescriptor descriptor, IndexQuery.RangePredicate<T1> predicate, IndexOrder indexOrder )
		 {
			  Value lower = predicate.FromValue();
			  Value upper = predicate.ToValue();
			  if ( lower == null || upper == null )
			  {
					throw new System.InvalidOperationException( "Use Values.NO_VALUE to encode the lack of a bound" );
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.NavigableMap<Neo4Net.values.storable.ValueTuple, ? extends Neo4Net.Kernel.Api.StorageEngine.TxState.LongDiffSets> sortedUpdates = txState.getSortedIndexUpdates(descriptor.schema());
			  NavigableMap<ValueTuple, ? extends LongDiffSets> sortedUpdates = txState.GetSortedIndexUpdates( descriptor.Schema() );
			  if ( sortedUpdates == null )
			  {
					return _emptyAddedAndRemovedWithValues;
			  }

			  ValueTuple selectedLower;
			  bool selectedIncludeLower;

			  ValueTuple selectedUpper;
			  bool selectedIncludeUpper;

			  if ( lower == NO_VALUE )
			  {
					selectedLower = ValueTuple.of( Values.minValue( predicate.ValueGroup(), upper ) );
					selectedIncludeLower = true;
			  }
			  else
			  {
					selectedLower = ValueTuple.of( lower );
					selectedIncludeLower = predicate.FromInclusive();
			  }

			  if ( upper == NO_VALUE )
			  {
					selectedUpper = ValueTuple.of( Values.maxValue( predicate.ValueGroup(), lower ) );
					selectedIncludeUpper = false;
			  }
			  else
			  {
					selectedUpper = ValueTuple.of( upper );
					selectedIncludeUpper = predicate.ToInclusive();
			  }

			  MutableList<NodeWithPropertyValues> added = Lists.mutable.empty();
			  MutableLongSet removed = LongSets.mutable.empty();

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Map<Neo4Net.values.storable.ValueTuple,? extends Neo4Net.Kernel.Api.StorageEngine.TxState.LongDiffSets> inRange = sortedUpdates.subMap(selectedLower, selectedIncludeLower, selectedUpper, selectedIncludeUpper);
			  IDictionary<ValueTuple, ? extends LongDiffSets> inRange = sortedUpdates.subMap( selectedLower, selectedIncludeLower, selectedUpper, selectedIncludeUpper );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (java.util.Map.Entry<Neo4Net.values.storable.ValueTuple,? extends Neo4Net.Kernel.Api.StorageEngine.TxState.LongDiffSets> entry : inRange.entrySet())
			  foreach ( KeyValuePair<ValueTuple, ? extends LongDiffSets> entry in inRange.SetOfKeyValuePairs() )
			  {
					ValueTuple values = entry.Key;
					Value[] valuesArray = values.Values;
					LongDiffSets diffForSpecificValue = entry.Value;

					// The TreeMap cannot perfectly order multi-dimensional types (spatial) and need additional filtering out false positives
					// TODO: If the composite index starts to be able to handle spatial types the line below needs enhancement
					if ( predicate.RegularOrder || predicate.AcceptsValue( values.OnlyValue ) )
					{
						 diffForSpecificValue.Added.each( nodeId => added.add( new NodeWithPropertyValues( nodeId, valuesArray ) ) );
						 removed.addAll( diffForSpecificValue.Removed );
					}
			  }
			  return new AddedWithValuesAndRemoved( indexOrder == IndexOrder.DESCENDING ? added.asReversed() : added, removed );
		 }

		 // PREFIX

		 internal static AddedAndRemoved IndexUpdatesForRangeSeekByPrefix( ReadableTransactionState txState, IndexDescriptor descriptor, TextValue prefix, IndexOrder indexOrder )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.NavigableMap<Neo4Net.values.storable.ValueTuple,? extends Neo4Net.Kernel.Api.StorageEngine.TxState.LongDiffSets> sortedUpdates = txState.getSortedIndexUpdates(descriptor.schema());
			  NavigableMap<ValueTuple, ? extends LongDiffSets> sortedUpdates = txState.GetSortedIndexUpdates( descriptor.Schema() );
			  if ( sortedUpdates == null )
			  {
					return _emptyAddedAndRemoved;
			  }
			  ValueTuple floor = ValueTuple.of( prefix );

			  MutableLongList added = LongLists.mutable.empty();
			  MutableLongSet removed = LongSets.mutable.empty();

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (java.util.Map.Entry<Neo4Net.values.storable.ValueTuple,? extends Neo4Net.Kernel.Api.StorageEngine.TxState.LongDiffSets> entry : sortedUpdates.subMap(floor, MAX_STRING_TUPLE).entrySet())
			  foreach ( KeyValuePair<ValueTuple, ? extends LongDiffSets> entry in sortedUpdates.subMap( floor, _maxStringTuple ).entrySet() )
			  {
					ValueTuple key = entry.Key;
					if ( ( ( TextValue ) key.OnlyValue ).startsWith( prefix ) )
					{
						 LongDiffSets diffSets = entry.Value;
						 added.addAll( diffSets.Added );
						 removed.addAll( diffSets.Removed );
					}
					else
					{
						 break;
					}
			  }
			  return new AddedAndRemoved( indexOrder == IndexOrder.DESCENDING ? added.asReversed() : added, removed );
		 }

		 internal static AddedWithValuesAndRemoved IndexUpdatesWithValuesForRangeSeekByPrefix( ReadableTransactionState txState, IndexDescriptor descriptor, TextValue prefix, IndexOrder indexOrder )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.NavigableMap<Neo4Net.values.storable.ValueTuple,? extends Neo4Net.Kernel.Api.StorageEngine.TxState.LongDiffSets> sortedUpdates = txState.getSortedIndexUpdates(descriptor.schema());
			  NavigableMap<ValueTuple, ? extends LongDiffSets> sortedUpdates = txState.GetSortedIndexUpdates( descriptor.Schema() );
			  if ( sortedUpdates == null )
			  {
					return _emptyAddedAndRemovedWithValues;
			  }
			  ValueTuple floor = ValueTuple.of( prefix );

			  MutableList<NodeWithPropertyValues> added = Lists.mutable.empty();
			  MutableLongSet removed = LongSets.mutable.empty();

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (java.util.Map.Entry<Neo4Net.values.storable.ValueTuple,? extends Neo4Net.Kernel.Api.StorageEngine.TxState.LongDiffSets> entry : sortedUpdates.tailMap(floor).entrySet())
			  foreach ( KeyValuePair<ValueTuple, ? extends LongDiffSets> entry in sortedUpdates.tailMap( floor ).entrySet() )
			  {
					ValueTuple key = entry.Key;
					if ( ( ( TextValue ) key.OnlyValue ).startsWith( prefix ) )
					{
						 LongDiffSets diffSets = entry.Value;
						 Value[] values = key.Values;
						 diffSets.Added.each( nodeId => added.add( new NodeWithPropertyValues( nodeId, values ) ) );
						 removed.addAll( diffSets.Removed );
					}
					else
					{
						 break;
					}
			  }
			  return new AddedWithValuesAndRemoved( indexOrder == IndexOrder.DESCENDING ? added.asReversed() : added, removed );
		 }

		 // HELPERS

		 private static AddedAndRemoved IndexUpdatesForScanAndFilter( ReadableTransactionState txState, IndexDescriptor descriptor, IndexQuery filter, IndexOrder indexOrder )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Map<Neo4Net.values.storable.ValueTuple,? extends Neo4Net.Kernel.Api.StorageEngine.TxState.LongDiffSets> updates = getUpdates(txState, descriptor, indexOrder);
			  IDictionary<ValueTuple, ? extends LongDiffSets> updates = GetUpdates( txState, descriptor, indexOrder );

			  if ( updates == null )
			  {
					return _emptyAddedAndRemoved;
			  }

			  MutableLongList added = LongLists.mutable.empty();
			  MutableLongSet removed = LongSets.mutable.empty();

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (java.util.Map.Entry<Neo4Net.values.storable.ValueTuple,? extends Neo4Net.Kernel.Api.StorageEngine.TxState.LongDiffSets> entry : updates.entrySet())
			  foreach ( KeyValuePair<ValueTuple, ? extends LongDiffSets> entry in updates.SetOfKeyValuePairs() )
			  {
					ValueTuple key = entry.Key;
					if ( filter == null || filter.AcceptsValue( key.OnlyValue ) )
					{
						 LongDiffSets diffSet = entry.Value;
						 added.addAll( diffSet.Added );
						 removed.addAll( diffSet.Removed );
					}
			  }
			  return new AddedAndRemoved( indexOrder == IndexOrder.DESCENDING ? added.asReversed() : added, removed );
		 }

		 private static AddedWithValuesAndRemoved IndexUpdatesWithValuesScanAndFilter( ReadableTransactionState txState, IndexDescriptor descriptor, IndexQuery filter, IndexOrder indexOrder )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Map<Neo4Net.values.storable.ValueTuple,? extends Neo4Net.Kernel.Api.StorageEngine.TxState.LongDiffSets> updates = getUpdates(txState, descriptor, indexOrder);
			  IDictionary<ValueTuple, ? extends LongDiffSets> updates = GetUpdates( txState, descriptor, indexOrder );

			  if ( updates == null )
			  {
					return _emptyAddedAndRemovedWithValues;
			  }

			  MutableList<NodeWithPropertyValues> added = Lists.mutable.empty();
			  MutableLongSet removed = LongSets.mutable.empty();

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (java.util.Map.Entry<Neo4Net.values.storable.ValueTuple,? extends Neo4Net.Kernel.Api.StorageEngine.TxState.LongDiffSets> entry : updates.entrySet())
			  foreach ( KeyValuePair<ValueTuple, ? extends LongDiffSets> entry in updates.SetOfKeyValuePairs() )
			  {
					ValueTuple key = entry.Key;
					if ( filter == null || filter.AcceptsValue( key.OnlyValue ) )
					{
						 Value[] values = key.Values;
						 LongDiffSets diffSet = entry.Value;
						 diffSet.Added.each( nodeId => added.add( new NodeWithPropertyValues( nodeId, values ) ) );
						 removed.addAll( diffSet.Removed );
					}
			  }
			  return new AddedWithValuesAndRemoved( indexOrder == IndexOrder.DESCENDING ? added.asReversed() : added, removed );
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private static java.util.Map<Neo4Net.values.storable.ValueTuple,? extends Neo4Net.Kernel.Api.StorageEngine.TxState.LongDiffSets> getUpdates(Neo4Net.Kernel.Api.StorageEngine.TxState.ReadableTransactionState txState, Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor descriptor, Neo4Net.Kernel.Api.Internal.IndexOrder indexOrder)
		 private static IDictionary<ValueTuple, ? extends LongDiffSets> GetUpdates( ReadableTransactionState txState, IndexDescriptor descriptor, IndexOrder indexOrder )
		 {
			  return indexOrder == IndexOrder.NONE ? txState.GetIndexUpdates( descriptor.Schema() ) : txState.GetSortedIndexUpdates(descriptor.Schema());
		 }

		 public class AddedAndRemoved
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly LongIterable AddedConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly LongSet RemovedConflict;

			  internal AddedAndRemoved( LongIterable added, LongSet removed )
			  {
					this.AddedConflict = added;
					this.RemovedConflict = removed;
			  }

			  public virtual bool Empty
			  {
				  get
				  {
						return AddedConflict.Empty && RemovedConflict.Empty;
				  }
			  }

			  public virtual LongIterable Added
			  {
				  get
				  {
						return AddedConflict;
				  }
			  }

			  public virtual LongSet Removed
			  {
				  get
				  {
						return RemovedConflict;
				  }
			  }
		 }

		 public class AddedWithValuesAndRemoved
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly IEnumerable<NodeWithPropertyValues> AddedConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly LongSet RemovedConflict;

			  internal AddedWithValuesAndRemoved( IEnumerable<NodeWithPropertyValues> added, LongSet removed )
			  {
					this.AddedConflict = added;
					this.RemovedConflict = removed;
			  }

			  public virtual bool Empty
			  {
				  get
				  {
						return !AddedConflict.GetEnumerator().hasNext() && RemovedConflict.Empty;
				  }
			  }

			  public virtual IEnumerable<NodeWithPropertyValues> Added
			  {
				  get
				  {
						return AddedConflict;
				  }
			  }

			  public virtual LongSet Removed
			  {
				  get
				  {
						return RemovedConflict;
				  }
			  }
		 }
	}

}