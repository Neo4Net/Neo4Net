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
namespace Neo4Net.Kernel.info
{

	public sealed class LockInfo
	{
		 private readonly string _resource;
		 private readonly string _description;
		 private readonly string _type;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ConstructorProperties({ "resourceType", "resourceId", "description" }) public LockInfo(String type, String resourceId, String description)
		 public LockInfo( string type, string resourceId, string description )
		 {
			  this._type = type;
			  this._resource = resourceId;
			  this._description = description;
		 }

		 public override string ToString()
		 {
			  return _description;
		 }

		 public string ResourceType
		 {
			 get
			 {
				  return _type;
			 }
		 }

		 public string ResourceId
		 {
			 get
			 {
				  return _resource;
			 }
		 }

		 public string Description
		 {
			 get
			 {
				  return _description;
			 }
		 }
	}

}