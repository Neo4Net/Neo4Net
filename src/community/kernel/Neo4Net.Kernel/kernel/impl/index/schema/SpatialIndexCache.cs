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

	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;

	/// <summary>
	/// Cache for lazily creating parts of the spatial index. Each part is created using the factory
	/// the first time it is selected in a select() query.
	/// 
	/// Iterating over the cache will return all currently created parts.
	/// </summary>
	/// @param <T> Type of parts </param>
	internal class SpatialIndexCache<T> : IndexPartsCache<CoordinateReferenceSystem, T>
	{
		 private readonly IFactory<T> _factory;

		 internal SpatialIndexCache( IFactory<T> factory )
		 {
			  this._factory = factory;
		 }

		 /// <summary>
		 /// Select the part corresponding to the given CoordinateReferenceSystem. Creates the part if needed,
		 /// and rethrows any create time exception as a RuntimeException.
		 /// </summary>
		 /// <param name="crs"> target coordinate reference system </param>
		 /// <returns> selected part </returns>
		 internal virtual T UncheckedSelect( CoordinateReferenceSystem crs )
		 {
			  T existing = Cache[crs];
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
					return Cache.computeIfAbsent(crs, key =>
					{
					 try
					 {
						  return _factory.newSpatial( crs );
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

		 /// <summary>
		 /// Select the part corresponding to the given CoordinateReferenceSystem. Creates the part if needed,
		 /// in which case an exception of type E might be thrown.
		 /// </summary>
		 /// <param name="crs"> target coordinate reference system </param>
		 /// <returns> selected part </returns>
		 internal virtual T Select( CoordinateReferenceSystem crs )
		 {
			  return UncheckedSelect( crs );
		 }

		 /// <summary>
		 /// Select the part corresponding to the given CoordinateReferenceSystem, apply function to it and return the result.
		 /// If the part isn't created yet return orElse.
		 /// </summary>
		 /// <param name="crs"> target coordinate reference system </param>
		 /// <param name="function"> function to apply to part </param>
		 /// <param name="orElse"> result to return if part isn't created yet </param>
		 /// @param <RESULT> type of result </param>
		 /// <returns> the result </returns>
		 internal virtual RESULT SelectOrElse<RESULT>( CoordinateReferenceSystem crs, System.Func<T, RESULT> function, RESULT orElse )
		 {
			  T part = Cache[crs];
			  if ( part == default( T ) )
			  {
					return orElse;
			  }
			  return function( part );
		 }

		 internal virtual void LoadAll()
		 {
			  foreach ( CoordinateReferenceSystem crs in CoordinateReferenceSystem.all() )
			  {
					UncheckedSelect( crs );
			  }
		 }

		 /// <summary>
		 /// Factory used by the SpatialIndexCache to create parts.
		 /// </summary>
		 /// @param <T> Type of parts </param>
		 internal interface IFactory<T>
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: T newSpatial(org.Neo4Net.values.storable.CoordinateReferenceSystem crs) throws java.io.IOException;
			  T NewSpatial( CoordinateReferenceSystem crs );
		 }
	}

}