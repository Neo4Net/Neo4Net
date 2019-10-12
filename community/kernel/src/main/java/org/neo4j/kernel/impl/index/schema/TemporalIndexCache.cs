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
namespace Org.Neo4j.Kernel.Impl.Index.Schema
{

	using Org.Neo4j.Function;
	using ValueGroup = Org.Neo4j.Values.Storable.ValueGroup;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.TemporalIndexCache.Offset.date;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.TemporalIndexCache.Offset.duration;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.TemporalIndexCache.Offset.localDateTime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.TemporalIndexCache.Offset.localTime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.TemporalIndexCache.Offset.zonedDateTime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.TemporalIndexCache.Offset.zonedTime;

	/// <summary>
	/// Cache for lazily creating parts of the temporal index. Each part is created using the factory
	/// the first time it is selected in a select() query, or the first time it's explicitly
	/// asked for using e.g. date().
	/// <para>
	/// Iterating over the cache will return all currently created parts.
	/// 
	/// </para>
	/// </summary>
	/// @param <T> Type of parts </param>
	internal class TemporalIndexCache<T> : IndexPartsCache<TemporalIndexCache.Offset, T>
	{
		 private readonly Factory<T> _factory;

		 internal enum Offset
		 {
			  Date,
			  LocalDateTime,
			  ZonedDateTime,
			  LocalTime,
			  ZonedTime,
			  Duration
		 }

		 internal TemporalIndexCache( Factory<T> factory )
		 {
			  this._factory = factory;
		 }

		 /// <summary>
		 /// Select the path corresponding to the given ValueGroup. Creates the path if needed,
		 /// and rethrows any create time exception as a RuntimeException.
		 /// </summary>
		 /// <param name="valueGroup"> target value group </param>
		 /// <returns> selected part </returns>
		 internal virtual T UncheckedSelect( ValueGroup valueGroup )
		 {
			  switch ( valueGroup.innerEnumValue )
			  {
			  case ValueGroup.InnerEnum.DATE:
					return Date();

			  case ValueGroup.InnerEnum.LOCAL_DATE_TIME:
					return LocalDateTime();

			  case ValueGroup.InnerEnum.ZONED_DATE_TIME:
					return ZonedDateTime();

			  case ValueGroup.InnerEnum.LOCAL_TIME:
					return LocalTime();

			  case ValueGroup.InnerEnum.ZONED_TIME:
					return ZonedTime();

			  case ValueGroup.InnerEnum.DURATION:
					return Duration();

			  default:
					throw new System.InvalidOperationException( "Unsupported value group " + valueGroup );
			  }
		 }

		 /// <summary>
		 /// Select the part corresponding to the given ValueGroup. Creates the part if needed,
		 /// in which case an exception of type E might be thrown.
		 /// </summary>
		 /// <param name="valueGroup"> target value group </param>
		 /// <returns> selected part </returns>
		 internal virtual T Select( ValueGroup valueGroup )
		 {
			  return UncheckedSelect( valueGroup );
		 }

		 /// <summary>
		 /// Select the part corresponding to the given ValueGroup, apply function to it and return the result.
		 /// If the part isn't created yet return orElse.
		 /// </summary>
		 /// <param name="valueGroup"> target value group </param>
		 /// <param name="function"> function to apply to part </param>
		 /// <param name="orElse"> result to return if part isn't created yet </param>
		 /// @param <RESULT> type of result </param>
		 /// <returns> the result </returns>
		 internal virtual RESULT SelectOrElse<RESULT>( ValueGroup valueGroup, System.Func<T, RESULT> function, RESULT orElse )
		 {
			  T cachedValue;
			  switch ( valueGroup.innerEnumValue )
			  {
			  case ValueGroup.InnerEnum.DATE:
					cachedValue = Cache[date];
					break;
			  case ValueGroup.InnerEnum.LOCAL_DATE_TIME:
					cachedValue = Cache[localDateTime];
					break;
			  case ValueGroup.InnerEnum.ZONED_DATE_TIME:
					cachedValue = Cache[zonedDateTime];
					break;
			  case ValueGroup.InnerEnum.LOCAL_TIME:
					cachedValue = Cache[localTime];
					break;
			  case ValueGroup.InnerEnum.ZONED_TIME:
					cachedValue = Cache[zonedTime];
					break;
			  case ValueGroup.InnerEnum.DURATION:
					cachedValue = Cache[duration];
					break;
			  default:
					throw new System.InvalidOperationException( "Unsupported value group " + valueGroup );
			  }

			  return cachedValue != default( T ) ? function( cachedValue ) : orElse;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private T getOrCreatePart(Offset key, org.neo4j.function.ThrowingSupplier<T,java.io.IOException> factory) throws java.io.UncheckedIOException
		 private T GetOrCreatePart( Offset key, ThrowingSupplier<T, IOException> factory )
		 {
			  T existing = Cache[key];
			  if ( existing != default( T ) )
			  {
					return existing;
			  }

			  // Instantiate from factory. Do this under lock so that we coordinate with any concurrent call to close.
			  // Concurrent calls to instantiating parts won't contend with each other since there's only
			  // a single writer at a time anyway.
			  InstantiateCloseLock.@lock();
			  try
			  {
					AssertOpen();
					return Cache.computeIfAbsent(key, k =>
					{
					 try
					 {
						  return factory.Get();
					 }
					 catch ( IOException e )
					 {
						  throw new UncheckedIOException( e );
					 }
					});
			  }
			  finally
			  {
					InstantiateCloseLock.unlock();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private T date() throws java.io.UncheckedIOException
		 private T Date()
		 {
			  return GetOrCreatePart( date, _factory.newDate );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private T localDateTime() throws java.io.UncheckedIOException
		 private T LocalDateTime()
		 {
			  return GetOrCreatePart( localDateTime, _factory.newLocalDateTime );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private T zonedDateTime() throws java.io.UncheckedIOException
		 private T ZonedDateTime()
		 {
			  return GetOrCreatePart( zonedDateTime, _factory.newZonedDateTime );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private T localTime() throws java.io.UncheckedIOException
		 private T LocalTime()
		 {
			  return GetOrCreatePart( localTime, _factory.newLocalTime );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private T zonedTime() throws java.io.UncheckedIOException
		 private T ZonedTime()
		 {
			  return GetOrCreatePart( zonedTime, _factory.newZonedTime );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private T duration() throws java.io.UncheckedIOException
		 private T Duration()
		 {
			  return GetOrCreatePart( duration, _factory.newDuration );
		 }

		 internal virtual void LoadAll()
		 {
			  try
			  {
					Date();
					ZonedDateTime();
					LocalDateTime();
					ZonedTime();
					LocalTime();
					Duration();
			  }
			  catch ( Exception e )
			  {
					throw new Exception( e );
			  }
		 }

		 /// <summary>
		 /// Factory used by the TemporalIndexCache to create parts.
		 /// </summary>
		 /// @param <T> Type of parts </param>
		 internal interface Factory<T>
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: T newDate() throws java.io.IOException;
			  T NewDate();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: T newLocalDateTime() throws java.io.IOException;
			  T NewLocalDateTime();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: T newZonedDateTime() throws java.io.IOException;
			  T NewZonedDateTime();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: T newLocalTime() throws java.io.IOException;
			  T NewLocalTime();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: T newZonedTime() throws java.io.IOException;
			  T NewZonedTime();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: T newDuration() throws java.io.IOException;
			  T NewDuration();
		 }
	}

}