using System;
using System.Text;

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
namespace Neo4Net.Kernel.Internal
{

	using Service = Neo4Net.Helpers.Service;

	public class Version : Service
	{
		 public static Version Kernel
		 {
			 get
			 {
				  return _kernelVersion;
			 }
		 }

		 public static string KernelVersion
		 {
			 get
			 {
				  return Kernel.getVersion();
			 }
		 }

		 public static string Neo4NetVersion
		 {
			 get
			 {
				  return Kernel.ReleaseVersion;
			 }
		 }

		 private readonly string _artifactId;
		 private readonly string _title;
		 private readonly string _vendor;
		 private readonly string _version;
		 private readonly string _releaseVersion;

		 public override string ToString()
		 {
			  StringBuilder result = new StringBuilder();
			  if ( !string.ReferenceEquals( _title, null ) )
			  {
					result.Append( _title );
					if ( string.ReferenceEquals( _artifactId, null ) || !_artifactId.Equals( _title ) )
					{
						 result.Append( " (" ).Append( _artifactId ).Append( ')' );
					}
			  }
			  else if ( !string.ReferenceEquals( _artifactId, null ) )
			  {
					result.Append( _artifactId );
			  }
			  else
			  {
					result.Append( "Unknown Component" );
			  }
			  result.Append( ", " );
			  if ( string.ReferenceEquals( _title, null ) )
			  {
					result.Append( "unpackaged " );
			  }
			  result.Append( "version: " ).Append( GetVersion() );
			  return result.ToString();
		 }

		 /// <returns> a detailed version string, including source control revision information if that is available, suitable
		 /// for internal use, logging and debugging. </returns>
		 public string GetVersion()
		 {
			  return _version;
		 }

		 /// <returns> a user-friendly version string, like "1.0.0-M01" or "2.0.0", suitable for end-user display </returns>
		 public virtual string ReleaseVersion
		 {
			 get
			 {
				  return _releaseVersion;
			 }
		 }

		 protected internal Version( string artifactId, string version ) : base( artifactId )
		 {
			  this._artifactId = artifactId;
			  this._title = artifactId;
			  this._vendor = "Neo Technology";
			  this._version = string.ReferenceEquals( version, null ) ? "dev" : version;
			  this._releaseVersion = ParseReleaseVersion( this._version );
		 }

		 /// <summary>
		 /// This reads out the user friendly part of the version, for public display.
		 /// </summary>
		 private string ParseReleaseVersion( string fullVersion )
		 {
			  // Generally, a version we extract from the jar manifest will look like:
			  //   1.2.3-M01,abcdef-dirty
			  // Parse out the first part of it:
			  Pattern pattern = Pattern.compile( "(\\d+" + "\\.\\d+" + "(\\.\\d+)?" + "(-?[^,]+)?)" + ".*" );

			  Matcher matcher = pattern.matcher( fullVersion );
			  if ( matcher.matches() )
			  {
					return matcher.group( 1 );
			  }

			  // If we don't recognize the version pattern, do the safe thing and keep it in full
			  return _version;
		 }

		 /// <summary>
		 /// A very nice to have main-method for quickly checking the version of a Neo4Net kernel,
		 /// for example given a kernel jar file.
		 /// </summary>
		 public static void Main( string[] args )
		 {
			  Version kernelVersion = Kernel;
			  Console.WriteLine( kernelVersion );
			  Console.WriteLine( "Title: " + kernelVersion._title );
			  Console.WriteLine( "Vendor: " + kernelVersion._vendor );
			  Console.WriteLine( "ArtifactId: " + kernelVersion._artifactId );
			  Console.WriteLine( "Version: " + kernelVersion.GetVersion() );
		 }

		 internal const string KERNEL_ARTIFACT_ID = "Neo4Net-kernel";
		 private static readonly Version _kernelVersion = new Version( KERNEL_ARTIFACT_ID, typeof( Version ).Assembly.ImplementationVersion );
	}

}