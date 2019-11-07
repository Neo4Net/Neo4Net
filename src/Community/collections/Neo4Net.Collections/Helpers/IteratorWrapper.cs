﻿using System.Collections.Generic;

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
namespace Neo4Net.Collections.Helpers
{

	/// <summary>
	/// Wraps an <seealso cref="System.Collections.IEnumerator"/> so that it returns items of another type. The
	/// iteration is done lazily.
	/// </summary>
	/// @param <T> the type of items to return </param>
	/// @param <U> the type of items to wrap/convert from </param>
	public abstract class IteratorWrapper<T, U> : IEnumerator<T>
	{
		 private IEnumerator<U> _source;

		 public IteratorWrapper( IEnumerator<U> iteratorToWrap )
		 {
			  this._source = iteratorToWrap;
		 }

		 public override bool HasNext()
		 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  return this._source.hasNext();
		 }

		 public override T Next()
		 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  return UnderlyingObjectToObject( this._source.next() );
		 }

		 public override void Remove()
		 {
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
			  this._source.remove();
		 }

		 protected internal abstract T UnderlyingObjectToObject( U @object );
	}

}