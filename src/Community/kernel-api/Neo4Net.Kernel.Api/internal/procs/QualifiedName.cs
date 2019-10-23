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
namespace Neo4Net.Kernel.Api.Internal.Procs
{

	using Iterables = Neo4Net.Helpers.Collections.Iterables;

	public class QualifiedName
	{
		 private readonly string[] @namespace;
		 private readonly string _name;

		 public QualifiedName( IList<string> @namespace, string name ) : this( @namespace.ToArray(), name )
		 {
		 }

		 public QualifiedName( string[] @namespace, string name )
		 {
			  this.@namespace = @namespace;
			  this._name = name;
		 }

		 public virtual string[] Namespace()
		 {
			  return @namespace;
		 }

		 public virtual string Name()
		 {
			  return _name;
		 }

		 public override string ToString()
		 {
			  string strNamespace = @namespace.Length > 0 ? Iterables.ToString( asList( @namespace ), "." ) + "." : "";
			  return string.Format( "{0}{1}", strNamespace, _name );
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }

			  QualifiedName that = ( QualifiedName ) o;
			  return Arrays.Equals( @namespace, that.@namespace ) && _name.Equals( that._name );
		 }

		 public override int GetHashCode()
		 {
			  int result = Arrays.GetHashCode( @namespace );
			  result = 31 * result + _name.GetHashCode();
			  return result;
		 }
	}

}