using System;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.backup.impl
{

	/// <summary>
	/// Contains a reference to a class (designed for enums) and can optionally also contain a throwable if the provided state has an exception attached
	/// </summary>
	/// @param <T> generic of an enum (not enforced) </param>
	internal class Fallible<T>
	{
		 private readonly T _state;
		 private readonly Exception _cause;

		 public virtual Optional<Exception> Cause
		 {
			 get
			 {
				  return Optional.ofNullable( _cause );
			 }
		 }

		 public virtual T State
		 {
			 get
			 {
				  return _state;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: Fallible(T state, @Nullable Throwable cause)
		 internal Fallible( T state, Exception cause )
		 {
			  this._state = state;
			  this._cause = cause;
		 }
	}

}