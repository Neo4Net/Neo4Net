using System;
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
namespace Neo4Net.Server.rest.repr
{

	using Neo4Net.Helpers.Collections;

	public class RepresentationExceptionHandlingIterable<T> : ExceptionHandlingIterable<T>
	{
		 public RepresentationExceptionHandlingIterable( IEnumerable<T> source ) : base( source )
		 {
		 }

		 protected internal override IEnumerator<T> ExceptionOnIterator( Exception t )
		 {
			  if ( t is Exception )
			  {
					Rethrow( new BadInputException( t ) );
			  }
			  return base.ExceptionOnIterator( t );
		 }

		 protected internal override T ExceptionOnNext( Exception t )
		 {
			  if ( t is Exception )
			  {
					Rethrow( new BadInputException( t ) );
			  }
			  return base.ExceptionOnNext( t );
		 }

		 protected internal override void ExceptionOnRemove( Exception t )
		 {
			  base.ExceptionOnRemove( t );
		 }

		 protected internal override bool ExceptionOnHasNext( Exception t )
		 {
			  if ( t is Exception )
			  {
					Rethrow( new BadInputException( t ) );
			  }
			  return base.ExceptionOnHasNext( t );
		 }
	}

}