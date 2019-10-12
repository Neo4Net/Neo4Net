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
namespace Neo4Net.@unsafe.Impl.Batchimport.staging
{

	using Predicates = Neo4Net.Function.Predicates;
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using Neo4Net.Kernel.impl.store;
	using AbstractBaseRecord = Neo4Net.Kernel.impl.store.record.AbstractBaseRecord;
	using RecordLoad = Neo4Net.Kernel.impl.store.record.RecordLoad;

	/// <summary>
	/// Convenience for reading and assembling records w/ potential filtering into an array.
	/// </summary>
	/// @param <RECORD> type of <seealso cref="AbstractBaseRecord"/>. </param>
	public class RecordDataAssembler<RECORD> where RECORD : Neo4Net.Kernel.impl.store.record.AbstractBaseRecord
	{
		 private readonly System.Func<RECORD> _factory;
		 private readonly Type<RECORD> _klass;
		 private readonly System.Predicate<RECORD> _filter;

		 public RecordDataAssembler( System.Func<RECORD> factory ) : this( factory, Predicates.alwaysTrue() )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public RecordDataAssembler(System.Func<RECORD> factory, System.Predicate<RECORD> filter)
		 public RecordDataAssembler( System.Func<RECORD> factory, System.Predicate<RECORD> filter )
		 {
			  this._factory = factory;
			  this._filter = filter;
			  this._klass = ( Type<RECORD> ) factory().GetType();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public RECORD[] newBatchObject(int batchSize)
		 public virtual RECORD[] NewBatchObject( int batchSize )
		 {
			  object array = Array.CreateInstance( _klass, batchSize );
			  for ( int i = 0; i < batchSize; i++ )
			  {
					( ( Array )array ).SetValue( _factory.get(), i );
			  }
			  return ( RECORD[] ) array;
		 }

		 public virtual bool Append( RecordStore<RECORD> store, PageCursor cursor, RECORD[] array, long id, int index )
		 {
			  RECORD record = array[index];
			  store.GetRecordByCursor( id, record, RecordLoad.CHECK, cursor );
			  return record.inUse() && _filter.test(record);
		 }

		 public virtual RECORD[] CutOffAt( RECORD[] array, int length )
		 {
			  for ( int i = length; i < array.Length; i++ )
			  {
					array[i].clear();
			  }
			  return array;
		 }
	}

}