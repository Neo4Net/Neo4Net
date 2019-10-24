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
namespace Neo4Net.Collections.Helpers
{

	internal class MapIterable<FROM, TO> : IEnumerable<TO>
	{
		 private readonly IEnumerable<FROM> _from;
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: private final System.Func<? super FROM,? extends TO> function;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 private readonly System.Func<object, ? extends TO> _function;

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: MapIterable(Iterable<FROM> from, System.Func<? super FROM,? extends TO> function)
		 internal MapIterable<T1>( IEnumerable<FROM> from, System.Func<T1> function ) where T1 : TO
		 {
			  this._from = from;
			  this._function = function;
		 }

		 public override IEnumerator<TO> Iterator()
		 {
			  return new MapIterator<TO>( _from.GetEnumerator(), _function );
		 }

		 internal class MapIterator<FROM, TO> : IEnumerator<TO>
		 {
			  internal readonly IEnumerator<FROM> FromIterator;
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: private final System.Func<? super FROM,? extends TO> function;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
			  internal readonly System.Func<object, ? extends TO> Function;

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: MapIterator(java.util.Iterator<FROM> fromIterator, System.Func<? super FROM,? extends TO> function)
			  internal MapIterator<T1>( IEnumerator<FROM> fromIterator, System.Func<T1> function ) where T1 : TO
			  {
					this.FromIterator = fromIterator;
					this.Function = function;
			  }

			  public override bool HasNext()
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					return FromIterator.hasNext();
			  }

			  public override TO Next()
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					FROM from = FromIterator.next();

					return Function.apply( from );
			  }

			  public override void Remove()
			  {
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
					FromIterator.remove();
			  }
		 }
	}

}