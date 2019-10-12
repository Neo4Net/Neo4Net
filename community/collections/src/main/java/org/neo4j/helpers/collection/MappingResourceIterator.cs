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
namespace Org.Neo4j.Helpers.Collection
{
	using Org.Neo4j.Graphdb;

	public abstract class MappingResourceIterator<T, S> : ResourceIterator<T>
	{
		public abstract ResourceIterator<R> Map( System.Func<T, R> map );
		public abstract java.util.stream.Stream<T> Stream();
		 private ResourceIterator<S> _sourceIterator;

		 public MappingResourceIterator( ResourceIterator<S> sourceResourceIterator )
		 {
			  this._sourceIterator = sourceResourceIterator;
		 }

		 protected internal abstract T Map( S @object );

		 public override bool HasNext()
		 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  return _sourceIterator.hasNext();
		 }

		 public override T Next()
		 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  return Map( _sourceIterator.next() );
		 }

		 public override void Remove()
		 {
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
			  _sourceIterator.remove();
		 }

		 public override void Close()
		 {
			  _sourceIterator.close();
		 }
	}

}