using System;
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
namespace Neo4Net.Kernel.Impl.Index.Schema.fusion
{

	using Neo4Net.Function;
	using Neo4Net.Function;
	using Exceptions = Neo4Net.Helpers.Exceptions;

	/// <summary>
	/// Selects an instance given a certain slot. </summary>
	/// @param <T> type of instance </param>
	internal class InstanceSelector<T>
	{
		 internal readonly Dictionary<IndexSlot, T> Instances;
		 internal bool Closed;

		 /// <summary>
		 /// Empty selector
		 /// </summary>
		 internal InstanceSelector() : this(new Dictionary<>(typeof(IndexSlot)))
		 {
		 }

		 /// <summary>
		 /// Selector with initial mapping
		 /// </summary>
		 /// <param name="map"> with initial mapping </param>
		 internal InstanceSelector( Dictionary<IndexSlot, T> map )
		 {
			  this.Instances = map;
		 }

		 /// <summary>
		 /// Put a new mapping to this selector
		 /// </summary>
		 internal virtual void Put( IndexSlot slot, T instance )
		 {
			  Instances[slot] = instance;
		 }

		 /// <summary>
		 /// Returns the instance at the given slot.
		 /// </summary>
		 /// <param name="slot"> slot number to return instance for. </param>
		 /// <returns> the instance at the given {@code slot}. </returns>
		 internal virtual T Select( IndexSlot slot )
		 {
			  if ( !Instances.ContainsKey( slot ) )
			  {
					throw new System.InvalidOperationException( "Instance is not instantiated" );
			  }
			  return Instances[slot];
		 }

		 /// <summary>
		 /// Map current instances to some other type using the converter function.
		 /// Even called on instances that haven't been instantiated yet.
		 /// Mapping is preserved in returned <seealso cref="System.Collections.Hashtable"/>.
		 /// </summary>
		 /// <param name="converter"> <seealso cref="ThrowingFunction"/> which converts from the source to target instance. </param>
		 /// @param <R> type of returned instance. </param>
		 /// @param <E> type of exception that converter may throw. </param>
		 /// <returns> A new <seealso cref="System.Collections.Hashtable"/> containing the mapped values. </returns>
		 /// <exception cref="E"> exception from converter. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: <R,E extends Exception> java.util.EnumMap<IndexSlot,R> map(org.neo4j.function.ThrowingFunction<T,R,E> converter) throws E
		 internal virtual Dictionary<IndexSlot, R> Map<R, E>( ThrowingFunction<T, R, E> converter ) where E : Exception
		 {
			  Dictionary<IndexSlot, R> result = new Dictionary<IndexSlot, R>( typeof( IndexSlot ) );
			  foreach ( IndexSlot slot in Enum.GetValues( typeof( IndexSlot ) ) )
			  {
					result[slot] = converter.Apply( Select( slot ) );
			  }
			  return result;
		 }

		 /// <summary>
		 /// Map current instances to some other type using the converter function,
		 /// without preserving the mapping.
		 /// Even called on instances that haven't been instantiated yet.
		 /// </summary>
		 /// <param name="converter"> <seealso cref="ThrowingFunction"/> which converts from the source to target instance. </param>
		 /// @param <R> type of returned instance. </param>
		 /// @param <E> type of exception that converter may throw. </param>
		 /// <returns> A new <seealso cref="System.Collections.Hashtable"/> containing the mapped values. </returns>
		 /// <exception cref="E"> exception from converter. </exception>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") <R,E extends Exception> Iterable<R> transform(org.neo4j.function.ThrowingFunction<T,R,E> converter) throws E
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual IEnumerable<R> Transform<R, E>( ThrowingFunction<T, R, E> converter ) where E : Exception
		 {
			  IList<R> result = new List<R>();
			  foreach ( IndexSlot slot in Enum.GetValues( typeof( IndexSlot ) ) )
			  {
					result.Add( converter.Apply( Select( slot ) ) );
			  }
			  return result;
		 }

		 /// <summary>
		 /// Convenience method for doing something to all instances, even those that haven't already been instantiated.
		 /// </summary>
		 /// <param name="consumer"> <seealso cref="Consumer"/> which performs some action on an instance. </param>
		 internal virtual void ForAll( System.Action<T> consumer )
		 {
			  Exception exception = null;
			  foreach ( IndexSlot slot in Enum.GetValues( typeof( IndexSlot ) ) )
			  {
					exception = ConsumeAndChainException( Select( slot ), consumer, exception );
			  }
			  if ( exception != null )
			  {
					throw exception;
			  }
		 }

		 /// <summary>
		 /// Perform a final action on instantiated instances and then closes this selector, preventing further instantiation.
		 /// </summary>
		 /// <param name="consumer"> <seealso cref="Consumer"/> which performs some action on an instance. </param>
		 internal virtual void Close( System.Action<T> consumer )
		 {
			  if ( !Closed )
			  {
					try
					{
						 ForInstantiated( consumer );
					}
					finally
					{
						 Closed = true;
					}
			  }
		 }

		 public override string ToString()
		 {
			  return Instances.ToString();
		 }

		 /// <summary>
		 /// Convenience method for doing something to already instantiated instances.
		 /// </summary>
		 /// <param name="consumer"> <seealso cref="ThrowingConsumer"/> which performs some action on an instance. </param>
		 private void ForInstantiated( System.Action<T> consumer )
		 {
			  Exception exception = null;
			  foreach ( T instance in Instances.Values )
			  {
					if ( instance != default( T ) )
					{
						 exception = ConsumeAndChainException( instance, consumer, exception );
					}
			  }
			  if ( exception != null )
			  {
					throw exception;
			  }
		 }

		 private Exception ConsumeAndChainException( T instance, System.Action<T> consumer, Exception exception )
		 {
			  try
			  {
					consumer( instance );
			  }
			  catch ( Exception e )
			  {
					exception = Exceptions.chain( exception, e );
			  }
			  return exception;
		 }
	}

}