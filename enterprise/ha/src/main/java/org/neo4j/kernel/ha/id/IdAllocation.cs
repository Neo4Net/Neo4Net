﻿/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
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
namespace Org.Neo4j.Kernel.ha.id
{
	using IdRange = Org.Neo4j.Kernel.impl.store.id.IdRange;

	public sealed class IdAllocation
	{
		 private readonly IdRange _idRange;
		 private readonly long _highestIdInUse;
		 private readonly long _defragCount;

		 public IdAllocation( IdRange idRange, long highestIdInUse, long defragCount )
		 {
			  this._idRange = idRange;
			  this._highestIdInUse = highestIdInUse;
			  this._defragCount = defragCount;
		 }

		 public long HighestIdInUse
		 {
			 get
			 {
				  return _highestIdInUse;
			 }
		 }

		 public long DefragCount
		 {
			 get
			 {
				  return _defragCount;
			 }
		 }

		 public IdRange IdRange
		 {
			 get
			 {
				  return this._idRange;
			 }
		 }

		 public override string ToString()
		 {
			  return "IdAllocation[" + _idRange + ", " + _highestIdInUse + ", " + _defragCount + "]";
		 }
	}

}