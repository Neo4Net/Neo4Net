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
namespace Neo4Net.Kernel.impl.locking
{

	using ResourceType = Neo4Net.Kernel.Api.StorageEngine.@lock.ResourceType;

	public interface ActiveLock
	{

		 string Mode();

		 ResourceType ResourceType();

		 long ResourceId();

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static ActiveLock exclusiveLock(org.Neo4Net.Kernel.Api.StorageEngine.@lock.ResourceType resourceType, long resourceId)
	//	 {
	//		  return new Implementation(resourceType, resourceId)
	//		  {
	//				@@Override public String mode()
	//				{
	//					 return EXCLUSIVE_MODE;
	//				}
	//		  };
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static ActiveLock sharedLock(org.Neo4Net.Kernel.Api.StorageEngine.@lock.ResourceType resourceType, long resourceId)
	//	 {
	//		  return new Implementation(resourceType, resourceId)
	//		  {
	//				@@Override public String mode()
	//				{
	//					 return SHARED_MODE;
	//				}
	//		  };
	//	 }
	}

	public static class ActiveLock_Fields
	{
		 public const string SHARED_MODE = "SHARED";
		 public const string EXCLUSIVE_MODE = "EXCLUSIVE";
	}

	 public interface ActiveLock_Factory
	 {

		  ActiveLock Create( ResourceType resourceType, long resourceId );
	 }

	 public static class ActiveLock_Factory_Fields
	 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
		  public static readonly ActiveLock_Factory SharedLock = ActiveLock::sharedLock;
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
		  public static readonly ActiveLock_Factory ExclusiveLock = ActiveLock::exclusiveLock;
	 }

	 public abstract class ActiveLock_Implementation : ActiveLock
	 {
		 public abstract ActiveLock SharedLock( ResourceType resourceType, long resourceId );
		 public abstract ActiveLock ExclusiveLock( ResourceType resourceType, long resourceId );
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		  internal readonly ResourceType ResourceTypeConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		  internal readonly long ResourceIdConflict;

		  internal ActiveLock_Implementation( ResourceType resourceType, long resourceId )
		  {
				this.ResourceTypeConflict = resourceType;
				this.ResourceIdConflict = resourceId;
		  }

		  public override abstract string Mode();

		  public override ResourceType ResourceType()
		  {
				return ResourceTypeConflict;
		  }

		  public override long ResourceId()
		  {
				return ResourceIdConflict;
		  }

		  public override bool Equals( object o )
		  {
				if ( this == o )
				{
					 return true;
				}
				if ( !( o is ActiveLock ) )
				{
					 return false;
				}
				ActiveLock that = ( ActiveLock ) o;
				return ResourceIdConflict == that.ResourceId() && Objects.Equals(Mode(), that.Mode()) && Objects.Equals(ResourceTypeConflict, that.ResourceType());
		  }

		  public override int GetHashCode()
		  {
				return Objects.hash( ResourceTypeConflict, ResourceIdConflict, Mode() );
		  }
	 }

}