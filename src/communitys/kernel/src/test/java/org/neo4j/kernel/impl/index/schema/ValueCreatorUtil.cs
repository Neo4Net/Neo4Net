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
namespace Neo4Net.Kernel.Impl.Index.Schema
{
	using ArrayUtils = org.apache.commons.lang3.ArrayUtils;


	using Neo4Net.Helpers.Collections;
	using IndexQuery = Neo4Net.@internal.Kernel.Api.IndexQuery;
	using Neo4Net.Kernel.Api.Index;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;
	using StoreIndexDescriptor = Neo4Net.Storageengine.Api.schema.StoreIndexDescriptor;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using RandomValues = Neo4Net.Values.Storable.RandomValues;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueType = Neo4Net.Values.Storable.ValueType;
	using Values = Neo4Net.Values.Storable.Values;

	internal class ValueCreatorUtil<KEY, VALUE> where KEY : NativeIndexKey<KEY> where VALUE : NativeIndexValue
	{
		 internal const double FRACTION_DUPLICATE_UNIQUE = 0;
		 internal const double FRACTION_DUPLICATE_NON_UNIQUE = 0.1;
		 private const double FRACTION_EXTREME_VALUE = 0.25;
		 private static readonly IComparer<IndexEntryUpdate<IndexDescriptor>> _updateComparator = ( u1, u2 ) => Values.COMPARATOR.Compare( u1.values()[0], u2.values()[0] );
		 private const int N_VALUES = 10;

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 internal readonly StoreIndexDescriptor IndexDescriptorConflict;
		 private readonly ValueType[] _supportedTypes;
		 private readonly double _fractionDuplicates;

		 internal ValueCreatorUtil( ValueCreatorUtil @delegate ) : this( @delegate.IndexDescriptorConflict, @delegate._supportedTypes, @delegate._fractionDuplicates )
		 {
		 }

		 internal ValueCreatorUtil( StoreIndexDescriptor indexDescriptor, ValueType[] supportedTypes, double fractionDuplicates )
		 {
			  this.IndexDescriptorConflict = indexDescriptor;
			  this._supportedTypes = supportedTypes;
			  this._fractionDuplicates = fractionDuplicates;
		 }

		 internal virtual int CompareIndexedPropertyValue( KEY key1, KEY key2 )
		 {
			  return Values.COMPARATOR.Compare( key1.asValues()[0], key2.asValues()[0] );
		 }

		 internal virtual ValueType[] SupportedTypes()
		 {
			  return _supportedTypes;
		 }

		 private double FractionDuplicates()
		 {
			  return _fractionDuplicates;
		 }

		 internal virtual IndexQuery RangeQuery( Value from, bool fromInclusive, Value to, bool toInclusive )
		 {
			  return IndexQuery.range( 0, from, fromInclusive, to, toInclusive );
		 }

		 internal virtual StoreIndexDescriptor IndexDescriptor()
		 {
			  return IndexDescriptorConflict;
		 }

		 internal virtual IndexEntryUpdate<IndexDescriptor>[] SomeUpdates( RandomRule randomRule )
		 {
			  return SomeUpdates( randomRule, SupportedTypes(), FractionDuplicates() );
		 }

		 internal virtual IndexEntryUpdate<IndexDescriptor>[] SomeUpdates( RandomRule random, ValueType[] types, bool allowDuplicates )
		 {
			  double fractionDuplicates = allowDuplicates ? FRACTION_DUPLICATE_NON_UNIQUE : FRACTION_DUPLICATE_UNIQUE;
			  return SomeUpdates( random, types, fractionDuplicates );
		 }

		 private IndexEntryUpdate<IndexDescriptor>[] SomeUpdates( RandomRule random, ValueType[] types, double fractionDuplicates )
		 {
			  RandomValueGenerator valueGenerator = new RandomValueGenerator( this, random.RandomValues(), types, fractionDuplicates );
			  RandomUpdateGenerator randomUpdateGenerator = new RandomUpdateGenerator( this, valueGenerator );
			  //noinspection unchecked
			  IndexEntryUpdate<IndexDescriptor>[] result = new IndexEntryUpdate[N_VALUES];
			  for ( int i = 0; i < N_VALUES; i++ )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					result[i] = randomUpdateGenerator.Next();
			  }
			  return result;
		 }

		 internal virtual IndexEntryUpdate<IndexDescriptor>[] SomeUpdatesWithDuplicateValues( RandomRule randomRule )
		 {
			  IEnumerator<Value> valueIterator = new RandomValueGenerator( this, randomRule.RandomValues(), SupportedTypes(), FractionDuplicates() );
			  Value[] someValues = new Value[N_VALUES];
			  for ( int i = 0; i < N_VALUES; i++ )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					someValues[i] = valueIterator.next();
			  }
			  return GenerateAddUpdatesFor( ArrayUtils.addAll( someValues, someValues ) );
		 }

		 internal virtual IEnumerator<IndexEntryUpdate<IndexDescriptor>> RandomUpdateGenerator( RandomRule randomRule )
		 {
			  return RandomUpdateGenerator( randomRule, SupportedTypes() );
		 }

		 internal virtual IEnumerator<IndexEntryUpdate<IndexDescriptor>> RandomUpdateGenerator( RandomRule random, ValueType[] types )
		 {
			  IEnumerator<Value> valueIterator = new RandomValueGenerator( this, random.RandomValues(), types, FractionDuplicates() );
			  return new RandomUpdateGenerator( this, valueIterator );
		 }

		 internal virtual IndexEntryUpdate<IndexDescriptor>[] GenerateAddUpdatesFor( Value[] values )
		 {
			  //noinspection unchecked
			  IndexEntryUpdate<IndexDescriptor>[] indexEntryUpdates = new IndexEntryUpdate[values.Length];
			  for ( int i = 0; i < indexEntryUpdates.Length; i++ )
			  {
					indexEntryUpdates[i] = Add( i, values[i] );
			  }
			  return indexEntryUpdates;
		 }

		 internal virtual Value[] ExtractValuesFromUpdates( IndexEntryUpdate<IndexDescriptor>[] updates )
		 {
			  Value[] values = new Value[updates.Length];
			  for ( int i = 0; i < updates.Length; i++ )
			  {
					if ( updates[i].Values().Length > 1 )
					{
						 throw new System.NotSupportedException( "This method does not support composite entries" );
					}
					values[i] = updates[i].Values()[0];
			  }
			  return values;
		 }

		 protected internal virtual IndexEntryUpdate<IndexDescriptor> Add( long nodeId, Value value )
		 {
			  return IndexEntryUpdate.add( nodeId, IndexDescriptorConflict, value );
		 }

		 internal static int CountUniqueValues( IndexEntryUpdate<IndexDescriptor>[] updates )
		 {
			  return Stream.of( updates ).map( update => update.values()[0] ).collect(Collectors.toSet()).size();
		 }

		 internal static int CountUniqueValues( Value[] updates )
		 {
			  ISet<Value> set = new SortedSet<Value>( Values.COMPARATOR );
			  set.addAll( Arrays.asList( updates ) );
			  return set.Count;
		 }

		 internal virtual void Sort( IndexEntryUpdate<IndexDescriptor>[] updates )
		 {
			  Arrays.sort( updates, _updateComparator );
		 }

		 internal virtual void CopyValue( VALUE value, VALUE intoValue )
		 { // no-op until we decide to use value for something
		 }

		 private class RandomValueGenerator : PrefetchingIterator<Value>
		 {
			 private readonly ValueCreatorUtil<KEY, VALUE> _outerInstance;

			  internal readonly ISet<Value> UniqueCompareValues;
			  internal readonly IList<Value> UniqueValues;
			  internal readonly ValueType[] Types;
			  internal readonly double FractionDuplicates;
			  internal readonly RandomValues RandomValues;

			  internal RandomValueGenerator( ValueCreatorUtil<KEY, VALUE> outerInstance, RandomValues randomValues, ValueType[] types, double fractionDuplicates )
			  {
				  this._outerInstance = outerInstance;
					this.Types = types;
					this.FractionDuplicates = fractionDuplicates;
					this.RandomValues = randomValues;
					this.UniqueCompareValues = new HashSet<Value>();
					this.UniqueValues = new List<Value>();
			  }

			  protected internal override Value FetchNextOrNull()
			  {
					Value value;
					if ( FractionDuplicates > 0 && UniqueValues.Count > 0 && RandomValues.nextFloat() < FractionDuplicates )
					{
						 value = RandomValues.among( UniqueValues );
					}
					else
					{
						 value = NewUniqueValue( RandomValues, UniqueCompareValues, UniqueValues );
					}

					return value;
			  }

			  internal virtual Value NewUniqueValue( RandomValues random, ISet<Value> uniqueCompareValues, IList<Value> uniqueValues )
			  {
					int attempts = 0;
					int maxAttempts = 10; // To avoid infinite loop on booleans
					Value value;
					do
					{
						 attempts++;
						 ValueType type = RandomValues.among( Types );
						 bool useExtremeValue = attempts == 1 && RandomValues.NextDouble() < FRACTION_EXTREME_VALUE;
						 if ( useExtremeValue )
						 {
							  value = RandomValues.among( type.extremeValues() );
						 }
						 else
						 {
							  value = random.NextValueOfType( type );
						 }
					} while ( attempts < maxAttempts && !uniqueCompareValues.Add( value ) );
					uniqueValues.Add( value );
					return value;
			  }
		 }

		 private class RandomUpdateGenerator : PrefetchingIterator<IndexEntryUpdate<IndexDescriptor>>
		 {
			 private readonly ValueCreatorUtil<KEY, VALUE> _outerInstance;

			  internal readonly IEnumerator<Value> ValueIterator;
			  internal long CurrentEntityId;

			  internal RandomUpdateGenerator( ValueCreatorUtil<KEY, VALUE> outerInstance, IEnumerator<Value> valueIterator )
			  {
				  this._outerInstance = outerInstance;
					this.ValueIterator = valueIterator;
			  }

			  protected internal override IndexEntryUpdate<IndexDescriptor> FetchNextOrNull()
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					Value value = ValueIterator.next();
					return outerInstance.Add( CurrentEntityId++, value );
			  }
		 }
	}

}