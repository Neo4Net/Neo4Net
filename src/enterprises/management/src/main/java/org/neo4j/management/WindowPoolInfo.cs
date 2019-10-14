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
namespace Neo4Net.management
{

	[Obsolete, Serializable]
	public sealed class WindowPoolInfo
	{
		 private const long SERIAL_VERSION_UID = 7743724554758487292L;

		 private string _name;
		 private long _memAvail;
		 private long _memUsed;
		 private int _windowCount;
		 private int _windowSize;
		 private int _hitCount;
		 private int _missCount;
		 private int _oomCount;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ConstructorProperties({ "windowPoolName", "availableMemory", "usedMemory", "numberOfWindows", "windowSize", "windowHitCount", "windowMissCount", "numberOfOutOfMemory" }) public WindowPoolInfo(String name, long memAvail, long memUsed, int windowCount, int windowSize, int hitCount, int missCount, int oomCount)
		 public WindowPoolInfo( string name, long memAvail, long memUsed, int windowCount, int windowSize, int hitCount, int missCount, int oomCount )
		 {
			  this._name = name;
			  this._memAvail = memAvail;
			  this._memUsed = memUsed;
			  this._windowCount = windowCount;
			  this._windowSize = windowSize;
			  this._hitCount = hitCount;
			  this._missCount = missCount;
			  this._oomCount = oomCount;
		 }

		 public string WindowPoolName
		 {
			 get
			 {
				  return _name;
			 }
		 }

		 public long AvailableMemory
		 {
			 get
			 {
				  return _memAvail;
			 }
		 }

		 public long UsedMemory
		 {
			 get
			 {
				  return _memUsed;
			 }
		 }

		 public int NumberOfWindows
		 {
			 get
			 {
				  return _windowCount;
			 }
		 }

		 public int WindowSize
		 {
			 get
			 {
				  return _windowSize;
			 }
		 }

		 public int WindowHitCount
		 {
			 get
			 {
				  return _hitCount;
			 }
		 }

		 public int WindowMissCount
		 {
			 get
			 {
				  return _missCount;
			 }
		 }

		 public int NumberOfOutOfMemory
		 {
			 get
			 {
				  return _oomCount;
			 }
		 }
	}

}