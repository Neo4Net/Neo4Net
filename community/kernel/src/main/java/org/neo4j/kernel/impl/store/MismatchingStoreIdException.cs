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
namespace Org.Neo4j.Kernel.impl.store
{
	using StoreId = Org.Neo4j.Storageengine.Api.StoreId;

	public class MismatchingStoreIdException : StoreFailureException
	{
		 private readonly StoreId _expected;
		 private readonly StoreId _encountered;

		 public MismatchingStoreIdException( StoreId expected, StoreId encountered ) : base( "Expected:" + expected + ", encountered:" + encountered )
		 {
			  this._expected = expected;
			  this._encountered = encountered;
		 }

		 public virtual StoreId Expected
		 {
			 get
			 {
				  return _expected;
			 }
		 }

		 public virtual StoreId Encountered
		 {
			 get
			 {
				  return _encountered;
			 }
		 }
	}

}