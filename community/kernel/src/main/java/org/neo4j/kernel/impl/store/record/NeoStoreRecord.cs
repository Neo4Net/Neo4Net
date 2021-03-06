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
namespace Org.Neo4j.Kernel.impl.store.record
{
	public class NeoStoreRecord : PrimitiveRecord
	{
		 public NeoStoreRecord() : base(-1)
		 {
			  InUse = true;
		 }

		 public override NeoStoreRecord Initialize( bool inUse, long nextProp )
		 {
			  base.Initialize( inUse, nextProp );
			  return this;
		 }

		 public override void Clear()
		 {
			  Initialize( false, -1 );
		 }

		 public override string ToString()
		 {
			  return this.GetType().Name + "[" +
						 "used=" + InUse() +
						 ",prop=" + NextProp +
						 "]";
		 }

		 public override PropertyRecord IdTo
		 {
			 set
			 {
			 }
		 }

		 public override NeoStoreRecord Clone()
		 {
			  NeoStoreRecord neoStoreRecord = new NeoStoreRecord();
			  neoStoreRecord.NextProp = NextProp;
			  return neoStoreRecord;
		 }
	}

}