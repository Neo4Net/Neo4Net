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
namespace Neo4Net.Kernel.impl.store
{
	using StoreId = Neo4Net.Storageengine.Api.StoreId;

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