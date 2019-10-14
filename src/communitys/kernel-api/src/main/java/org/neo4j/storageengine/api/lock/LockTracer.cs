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
namespace Neo4Net.Storageengine.Api.@lock
{
	public interface LockTracer
	{
		 LockWaitEvent WaitForLock( bool exclusive, ResourceType resourceType, params long[] resourceIds );

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default LockTracer combine(LockTracer tracer)
	//	 {
	//		  if (tracer == NONE)
	//		  {
	//				return this;
	//		  }
	//		  return new CombinedTracer(this, tracer);
	//	 }

	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 LockTracer NONE = new LockTracer()
	//	 {
	//		  @@Override public LockWaitEvent waitForLock(boolean exclusive, ResourceType resourceType, long... resourceIds)
	//		  {
	//				return LockWaitEvent.NONE;
	//		  }
	//
	//		  @@Override public LockTracer combine(LockTracer tracer)
	//		  {
	//				return tracer;
	//		  }
	//	 };
	}

}