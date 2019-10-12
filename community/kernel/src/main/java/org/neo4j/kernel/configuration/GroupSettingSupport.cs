using System;

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
namespace Org.Neo4j.Kernel.configuration
{
	using Org.Neo4j.Graphdb.config;

	/// <summary>
	/// This class helps you implement grouped settings without exposing internal utility methods
	/// in public APIs - eg. this class is not public, and because you use delegation rather than
	/// subclassing to use it, we don't end up exposing this class publicly.
	/// </summary>
	public class GroupSettingSupport
	{
		 private readonly string _groupName;
		 public readonly string GroupKey;

		 private static string GroupPrefix( Type groupClass )
		 {
			  return groupClass.getAnnotation( typeof( Group ) ).value();
		 }

		 public GroupSettingSupport( Type groupClass, string groupKey ) : this( GroupPrefix( groupClass ), groupKey )
		 {
		 }

		 /// <param name="groupPrefix"> the base that is common for each group of this kind, eg. 'dbms.mygroup' </param>
		 /// <param name="groupKey"> the unique key for this particular group instance, eg. '0' or 'group1',
		 ///                 this gets combined with the groupPrefix to eg. `dbms.mygroup.0` </param>
		 private GroupSettingSupport( string groupPrefix, string groupKey )
		 {
			  this.GroupKey = groupKey;
			  this._groupName = groupPrefix + "." + groupKey;
		 }

		 /// <summary>
		 /// Define a sub-setting of this group. The setting passed in should not worry about
		 /// the group prefix or key. If you want config like `dbms.mygroup.0.foo=bar`, you should
		 /// pass in a setting with the key `foo` here.
		 /// </summary>
		 public virtual Setting<T> Scope<T>( Setting<T> setting )
		 {
			  setting.WithScope( key => _groupName + "." + key );
			  return setting;
		 }
	}

}