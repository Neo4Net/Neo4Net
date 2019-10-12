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
namespace Neo4Net.Io.os
{

	/// <summary>
	/// Utility class that exposes methods from proprietary implementations of <seealso cref="OperatingSystemMXBean"/>.
	/// Able to work on Oracle JDK and IBM JDK.
	/// Methods never fail but instead return <seealso cref="VALUE_UNAVAILABLE"/> if such method is not exposed by the underlying
	/// MX bean.
	/// </summary>
	public sealed class OsBeanUtil
	{
		 public const long VALUE_UNAVAILABLE = -1;

		 private const string SUN_OS_BEAN = "com.sun.management.OperatingSystemMXBean";
		 private const string SUN_UNIX_OS_BEAN = "com.sun.management.UnixOperatingSystemMXBean";
		 private const string IBM_OS_BEAN = "com.ibm.lang.management.OperatingSystemMXBean";

		 private static readonly OperatingSystemMXBean _osBean = ManagementFactory.OperatingSystemMXBean;

		 private static readonly System.Reflection.MethodInfo _getTotalPhysicalMemoryMethod;
		 private static readonly System.Reflection.MethodInfo _getFreePhysicalMemoryMethod;
		 private static readonly System.Reflection.MethodInfo _getCommittedVirtualMemoryMethod;
		 private static readonly System.Reflection.MethodInfo _getTotalSwapSpaceMethod;
		 private static readonly System.Reflection.MethodInfo _getFreeSwapSpaceMethod;
		 private static readonly System.Reflection.MethodInfo _getMaxFileDescriptorsMethod;
		 private static readonly System.Reflection.MethodInfo _getOpenFileDescriptorsMethod;

		 static OsBeanUtil()
		 {
			  _getTotalPhysicalMemoryMethod = FindOsBeanMethod( "getTotalPhysicalMemorySize", "getTotalPhysicalMemory" );
			  _getFreePhysicalMemoryMethod = FindOsBeanMethod( "getFreePhysicalMemorySize", "getFreePhysicalMemorySize" );
			  _getCommittedVirtualMemoryMethod = FindOsBeanMethod( "getCommittedVirtualMemorySize", null );
			  _getTotalSwapSpaceMethod = FindOsBeanMethod( "getTotalSwapSpaceSize", "getTotalSwapSpaceSize" );
			  _getFreeSwapSpaceMethod = FindOsBeanMethod( "getFreeSwapSpaceSize", "getFreeSwapSpaceSize" );
			  _getMaxFileDescriptorsMethod = FindUnixOsBeanMethod( "getMaxFileDescriptorCount" );
			  _getOpenFileDescriptorsMethod = FindUnixOsBeanMethod( "getOpenFileDescriptorCount" );
		 }

		 private OsBeanUtil()
		 {
			  throw new AssertionError( "Not for instantiation!" );
		 }

		 /// <returns> total amount of physical memory in bytes, or <seealso cref="VALUE_UNAVAILABLE"/> if underlying bean does not
		 /// provide this functionality. </returns>
		 public static long TotalPhysicalMemory
		 {
			 get
			 {
				  return Invoke( _getTotalPhysicalMemoryMethod );
			 }
		 }

		 /// <returns> amount of free physical memory in bytes, or <seealso cref="VALUE_UNAVAILABLE"/> if underlying bean does not
		 /// provide this functionality. </returns>
		 public static long FreePhysicalMemory
		 {
			 get
			 {
				  return Invoke( _getFreePhysicalMemoryMethod );
			 }
		 }

		 /// <returns> amount of virtual memory that is guaranteed to be available to the running process in bytes, or
		 /// <seealso cref="VALUE_UNAVAILABLE"/> if underlying bean does not provide this functionality. </returns>
		 public static long CommittedVirtualMemory
		 {
			 get
			 {
				  return Invoke( _getCommittedVirtualMemoryMethod );
			 }
		 }

		 /// <returns> total amount of swap space in bytes, or <seealso cref="VALUE_UNAVAILABLE"/> if underlying bean does not
		 /// provide this functionality. </returns>
		 public static long TotalSwapSpace
		 {
			 get
			 {
				  return Invoke( _getTotalSwapSpaceMethod );
			 }
		 }

		 /// <returns> total amount of free swap space in bytes, or <seealso cref="VALUE_UNAVAILABLE"/> if underlying bean does not
		 /// provide this functionality. </returns>
		 public static long FreeSwapSpace
		 {
			 get
			 {
				  return Invoke( _getFreeSwapSpaceMethod );
			 }
		 }

		 /// <returns> maximum number of file descriptors, or <seealso cref="VALUE_UNAVAILABLE"/> if underlying bean does not
		 /// provide this functionality. </returns>
		 public static long MaxFileDescriptors
		 {
			 get
			 {
				  return Invoke( _getMaxFileDescriptorsMethod );
			 }
		 }

		 /// <returns> number of open file descriptors, or <seealso cref="VALUE_UNAVAILABLE"/> if underlying bean does not
		 /// provide this functionality. </returns>
		 public static long OpenFileDescriptors
		 {
			 get
			 {
				  return Invoke( _getOpenFileDescriptorsMethod );
			 }
		 }

		 private static System.Reflection.MethodInfo FindOsBeanMethod( string sunMethodName, string ibmMethodName )
		 {
			  System.Reflection.MethodInfo sunOsBeanMethod = FindSunOsBeanMethod( sunMethodName );
			  return sunOsBeanMethod == null ? FindIbmOsBeanMethod( ibmMethodName ) : sunOsBeanMethod;
		 }

		 private static System.Reflection.MethodInfo FindUnixOsBeanMethod( string methodName )
		 {
			  return FindMethod( SUN_UNIX_OS_BEAN, methodName );
		 }

		 private static System.Reflection.MethodInfo FindSunOsBeanMethod( string methodName )
		 {
			  return FindMethod( SUN_OS_BEAN, methodName );
		 }

		 private static System.Reflection.MethodInfo FindIbmOsBeanMethod( string methodName )
		 {
			  return FindMethod( IBM_OS_BEAN, methodName );
		 }

		 private static System.Reflection.MethodInfo FindMethod( string className, string methodName )
		 {
			  try
			  {
					return ( string.ReferenceEquals( methodName, null ) ) ? null : Type.GetType( className ).GetMethod( methodName );
			  }
			  catch ( Exception )
			  {
					return null;
			  }
		 }

		 private static long Invoke( System.Reflection.MethodInfo method )
		 {
			  try
			  {
					object value = ( method == null ) ? null : method.invoke( _osBean );
					return ( value == null ) ? VALUE_UNAVAILABLE : ( ( Number ) value ).longValue();
			  }
			  catch ( Exception )
			  {
					return VALUE_UNAVAILABLE;
			  }
		 }
	}

}