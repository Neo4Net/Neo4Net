using System.Collections.Generic;

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
namespace Neo4Net.Server.configuration
{
	public class ThirdPartyJaxRsPackage
	{
		 private readonly string _packageName;

		 private class FuncAnonymousInnerClass : System.Func<string, IList<ThirdPartyJaxRsPackage>>
		 {
			 public override IList<ThirdPartyJaxRsPackage> apply( string value )
			 {
				  string[] list = value.Split( Settings.SEPARATOR, true );
				  IList<ThirdPartyJaxRsPackage> result = new List<ThirdPartyJaxRsPackage>();
				  foreach ( string item in list )
				  {
						item = item.Trim();
						if ( !item.Equals( "" ) )
						{
							 result.add( createThirdPartyJaxRsPackage( item ) );
						}
				  }
				  return result;
			 }

			 public override string ToString()
			 {
				  return "a comma-separated list of <classname>=<mount point> strings";
			 }

			 private ThirdPartyJaxRsPackage createThirdPartyJaxRsPackage( string packageAndMountpoint )
			 {
				  string[] parts = packageAndMountpoint.Split( "=", true );
				  if ( parts.Length != 2 )
				  {
						throw new System.ArgumentException( "config for " + ServerSettings.third_party_packages.name() + " is wrong: " + packageAndMountpoint );
				  }
				  string pkg = parts[0];
				  string mountPoint = parts[1];
				  return new ThirdPartyJaxRsPackage( pkg, mountPoint );
			 }
		 }
		 private readonly string _mountPoint;

		 public ThirdPartyJaxRsPackage( string packageName, string mountPoint )
		 {
			  this._packageName = packageName;
			  this._mountPoint = mountPoint;
		 }

		 public virtual string PackageName
		 {
			 get
			 {
				  return _packageName;
			 }
		 }

		 public virtual string MountPoint
		 {
			 get
			 {
				  return _mountPoint;
			 }
		 }
	}

}