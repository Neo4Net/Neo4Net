using System;

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
namespace Org.Neo4j.Function
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;

	/// <summary>
	/// Constructors for basic <seealso cref="Supplier"/> types
	/// </summary>
	public sealed class Suppliers
	{
		 private Suppliers()
		 {
		 }

		 /// <summary>
		 /// Creates a <seealso cref="Supplier"/> that returns a single object
		 /// </summary>
		 /// <param name="instance"> The object to return </param>
		 /// @param <T> The object type </param>
		 /// <returns> A <seealso cref="Supplier"/> returning the specified object instance </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <T> System.Func<T> singleton(final T instance)
		 public static System.Func<T> Singleton<T>( T instance )
		 {
			  return () => instance;
		 }

		 /// <summary>
		 /// Creates a lazy initialized <seealso cref="Supplier"/> of a single object
		 /// </summary>
		 /// <param name="supplier"> A supplier that will provide the object when required </param>
		 /// @param <T> The object type </param>
		 /// <returns> A <seealso cref="Supplier"/> returning the specified object instance </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <T> Lazy<T> lazySingleton(final System.Func<T> supplier)
		 public static Lazy<T> LazySingleton<T>( System.Func<T> supplier )
		 {
			  return new LazyAnonymousInnerClass( supplier );
		 }

		 private class LazyAnonymousInnerClass : Lazy<T>
		 {
			 private System.Func<T> _supplier;

			 public LazyAnonymousInnerClass( System.Func<T> supplier )
			 {
				 this._supplier = supplier;
			 }

			 internal volatile T instance;

			 public T get()
			 {
				  if ( Initialised )
				  {
						return instance;
				  }

				  lock ( this )
				  {
						if ( instance == null )
						{
							 instance = _supplier();
						}
				  }
				  return instance;
			 }

			 public bool Initialised
			 {
				 get
				 {
					  return instance != null;
				 }
			 }
		 }

		 /// <summary>
		 /// Creates a new <seealso cref="Supplier"/> that applies the specified function to the values obtained from a source supplier. The
		 /// function is only invoked once for every sequence of identical objects obtained from the source supplier (the previous result
		 /// is cached and returned again if the source object hasn't changed).
		 /// </summary>
		 /// <param name="supplier"> A supplier of source objects </param>
		 /// <param name="adaptor"> A function mapping source objects to result objects </param>
		 /// @param <V> The source object type </param>
		 /// @param <T> The result object type </param>
		 /// <returns> A <seealso cref="Supplier"/> of objects </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <T, V> System.Func<T> adapted(final System.Func<V> supplier, final System.Func<V,T> adaptor)
		 public static System.Func<T> Adapted<T, V>( System.Func<V> supplier, System.Func<V, T> adaptor )
		 {
			  return new FuncAnonymousInnerClass( supplier, adaptor );
		 }

		 private class FuncAnonymousInnerClass : System.Func<T>
		 {
			 private System.Func<V> _supplier;
			 private System.Func<V, T> _adaptor;

			 public FuncAnonymousInnerClass( System.Func<V> supplier, System.Func<V, T> adaptor )
			 {
				 this._supplier = supplier;
				 this._adaptor = adaptor;
			 }

			 internal volatile V lastValue;
			 internal T instance;

			 public override T get()
			 {
				  V value = _supplier();
				  if ( value == lastValue )
				  {
						return instance;
				  }

				  T adaptedValue = _adaptor( value );
				  lock ( this )
				  {
						if ( value != lastValue )
						{
							 instance = adaptedValue;
							 lastValue = value;
						}
				  }
				  return instance;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <T, E extends Exception> ThrowingCapturingSupplier<T,E> compose(final ThrowingSupplier<T,? extends E> input, final ThrowingPredicate<T,? extends E> predicate)
		 public static ThrowingCapturingSupplier<T, E> Compose<T, E, T1, T2>( ThrowingSupplier<T1> input, ThrowingPredicate<T2> predicate ) where E : Exception where T1 : E where T2 : E
		 {
			  return new ThrowingCapturingSupplier<T, E>( input, predicate );
		 }

		 public static System.Func<bool> UntilTimeExpired( long duration, TimeUnit unit )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long endTimeInMilliseconds = currentTimeMillis() + unit.toMillis(duration);
			  long endTimeInMilliseconds = currentTimeMillis() + unit.toMillis(duration);
			  return () => currentTimeMillis() <= endTimeInMilliseconds;
		 }

		 internal class ThrowingCapturingSupplier<T, E> : ThrowingSupplier<bool, E> where E : Exception
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final ThrowingSupplier<T,? extends E> input;
			  internal readonly ThrowingSupplier<T, ? extends E> Input;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final ThrowingPredicate<T,? extends E> predicate;
			  internal readonly ThrowingPredicate<T, ? extends E> Predicate;

			  internal T Current;

			  internal ThrowingCapturingSupplier<T1, T2>( ThrowingSupplier<T1> input, ThrowingPredicate<T2> predicate ) where T1 : E where T2 : E
			  {
					this.Input = input;
					this.Predicate = predicate;
			  }

			  internal virtual T LastInput()
			  {
					return Current;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public System.Nullable<bool> get() throws E
			  public override bool? Get()
			  {
					Current = Input.get();
					return Predicate.test( Current );
			  }

			  public override string ToString()
			  {
					return string.Format( "{0} on {1}", Predicate, Input );
			  }
		 }

		 public interface Lazy<T> : System.Func<T>
		 {
			  bool Initialised { get; }
		 }
	}

}