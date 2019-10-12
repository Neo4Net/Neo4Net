/*
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
namespace Neo4Net.@unsafe.Impl.Batchimport.cache.idmapping.@string
{
	using Group = Neo4Net.@unsafe.Impl.Batchimport.input.Group;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.Numbers.safeCastIntToUnsignedByte;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.Numbers.safeCastIntToUnsignedShort;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.Numbers.unsignedByteToInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.Numbers.unsignedShortToInt;

	/// <summary>
	/// Cache for keeping nodeId --> groupId mapping.
	/// </summary>
	public interface GroupCache : AutoCloseable
	{
		 void Set( long nodeId, int groupId );

		 int Get( long nodeId );

		 void Close();

	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 GroupCache GLOBAL = new GroupCache()
	//	 {
	//		  @@Override public void set(long nodeId, int groupId)
	//		  { // no need
	//				assert groupId == Group.GLOBAL.id();
	//		  }
	//
	//		  @@Override public int get(long nodeId)
	//		  {
	//				return Group.GLOBAL.id();
	//		  }
	//
	//		  @@Override public void close()
	//		  {
	//		  }
	//	 };

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static GroupCache select(org.neo4j.@unsafe.impl.batchimport.cache.NumberArrayFactory factory, int chunkSize, int numberOfGroups)
	//	 {
	//		  if (numberOfGroups == 0)
	//		  {
	//				return GLOBAL;
	//		  }
	//		  if (numberOfGroups <= 0x100)
	//		  {
	//				return new ByteGroupCache(factory, chunkSize);
	//		  }
	//		  if (numberOfGroups <= 0x10000)
	//		  {
	//				return new ShortGroupCache(factory, chunkSize);
	//		  }
	//		  throw new IllegalArgumentException("Max allowed groups is " + 0xFFFF + ", but wanted " + numberOfGroups);
	//	 }
	}

	 public class GroupCache_ByteGroupCache : GroupCache
	 {
		  internal readonly ByteArray Array;

		  public GroupCache_ByteGroupCache( NumberArrayFactory factory, int chunkSize )
		  {
				Array = factory.NewDynamicByteArray( chunkSize, new sbyte[Byte.BYTES] );
		  }

		  public override void Set( long nodeId, int groupId )
		  {
				Array.setByte( nodeId, 0, safeCastIntToUnsignedByte( groupId ) );
		  }

		  public override int Get( long nodeId )
		  {
				return unsignedByteToInt( Array.getByte( nodeId, 0 ) );
		  }

		  public override void Close()
		  {
				Array.close();
		  }
	 }

	 public class GroupCache_ShortGroupCache : GroupCache
	 {
		  internal readonly ByteArray Array;

		  public GroupCache_ShortGroupCache( NumberArrayFactory factory, int chunkSize )
		  {
				Array = factory.NewDynamicByteArray( chunkSize, new sbyte[Short.BYTES] );
		  }

		  public override void Set( long nodeId, int groupId )
		  {
				Array.setShort( nodeId, 0, safeCastIntToUnsignedShort( groupId ) );
		  }

		  public override int Get( long nodeId )
		  {
				return unsignedShortToInt( Array.getShort( nodeId, 0 ) );
		  }

		  public override void Close()
		  {
				Array.close();
		  }
	 }

}