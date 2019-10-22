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
namespace Neo4Net.Kernel.api.proc
{

	using ProcedureException = Neo4Net.Internal.Kernel.Api.exceptions.ProcedureException;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;

	/// <summary>
	/// Not thread safe. Basic context backed by a map.
	/// </summary>
	public class BasicContext : Context
	{
		 private readonly IDictionary<string, object> _values = new Dictionary<string, object>();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <T> T get(Key<T> key) throws org.Neo4Net.internal.kernel.api.exceptions.ProcedureException
		 public override T Get<T>( Key<T> key )
		 {
			  object o = _values[key.Name()];
			  if ( o == null )
			  {
					throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureCallFailed, "There is no `%s` in the current procedure call context.", key.Name() );
			  }
			  return ( T ) o;
		 }

		 public override T GetOrElse<T>( Key<T> key, T orElse )
		 {
			  object o = _values[key.Name()];
			  if ( o == null )
			  {
					return orElse;
			  }
			  return ( T ) o;
		 }

		 public virtual void Put<T>( Key<T> key, T value )
		 {
			  _values[key.Name()] = value;
		 }
	}

}