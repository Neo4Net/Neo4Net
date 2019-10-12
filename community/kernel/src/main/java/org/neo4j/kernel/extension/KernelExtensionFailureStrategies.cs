using System;
using System.Text;

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
namespace Org.Neo4j.Kernel.extension
{

	using UnsatisfiedDependencyException = Org.Neo4j.Kernel.impl.util.UnsatisfiedDependencyException;

	public class KernelExtensionFailureStrategies
	{
		 private KernelExtensionFailureStrategies()
		 {
		 }

		 private static FailedToBuildKernelExtensionException Wrap( KernelExtensionFactory kernelExtensionFactory, UnsatisfiedDependencyException e )
		 {
			  return new FailedToBuildKernelExtensionException( "Failed to build kernel extension " + kernelExtensionFactory + " due to a missing dependency: " + e.Message, e );
		 }

		 private static FailedToBuildKernelExtensionException Wrap( KernelExtensionFactory kernelExtensionFactory, Exception e )
		 {
			  StringBuilder message = ( new StringBuilder( "Failed to build kernel extension " ) ).Append( kernelExtensionFactory );
			  if ( e is LinkageError || e is ReflectiveOperationException )
			  {
					if ( e is LinkageError )
					{
						 message.Append( " because it is compiled with a reference to a class, method, or field, that is not in the class path: " );
					}
					else
					{
						 message.Append( " because it a reflective access to a class, method, or field, that is not in the class path: " );
					}
					message.Append( '\'' ).Append( e.Message ).Append( '\'' );
					message.Append( ". The most common cause of this problem, is that Neo4j has been upgraded without also upgrading all" );
					message.Append( "installed extensions, such as APOC. " );
					message.Append( "Make sure that all of your extensions are build against your specific version of Neo4j." );
			  }
			  else
			  {
					message.Append( " because of an unanticipated error: '" ).Append( e.Message ).Append( "'." );
			  }
			  return new FailedToBuildKernelExtensionException( message.ToString(), e );
		 }

		 public static KernelExtensionFailureStrategy Fail()
		 {
			  return new KernelExtensionFailureStrategyAnonymousInnerClass();
		 }

		 private class KernelExtensionFailureStrategyAnonymousInnerClass : KernelExtensionFailureStrategy
		 {
			 public void handle( KernelExtensionFactory kernelExtensionFactory, UnsatisfiedDependencyException e )
			 {
				  throw Wrap( kernelExtensionFactory, e );
			 }

			 public void handle( KernelExtensionFactory kernelExtensionFactory, Exception e )
			 {
				  throw Wrap( kernelExtensionFactory, e );
			 }
		 }

		 public static KernelExtensionFailureStrategy Ignore()
		 {
			  return new KernelExtensionFailureStrategyAnonymousInnerClass2();
		 }

		 private class KernelExtensionFailureStrategyAnonymousInnerClass2 : KernelExtensionFailureStrategy
		 {
			 public void handle( KernelExtensionFactory kernelExtensionFactory, UnsatisfiedDependencyException e )
			 {
				  // Just ignore.
			 }

			 public void handle( KernelExtensionFactory kernelExtensionFactory, Exception e )
			 {
				  // Just ignore.
			 }
		 }

		 // Perhaps not used, but very useful for debugging kernel extension loading problems
		 public static KernelExtensionFailureStrategy Print( PrintStream @out )
		 {
			  return new KernelExtensionFailureStrategyAnonymousInnerClass3( @out );
		 }

		 private class KernelExtensionFailureStrategyAnonymousInnerClass3 : KernelExtensionFailureStrategy
		 {
			 private PrintStream @out;

			 public KernelExtensionFailureStrategyAnonymousInnerClass3( PrintStream @out )
			 {
				 this.@out = @out;
			 }

			 public void handle( KernelExtensionFactory kernelExtensionFactory, UnsatisfiedDependencyException e )
			 {
				  Wrap( kernelExtensionFactory, e ).printStackTrace( @out );
			 }

			 public void handle( KernelExtensionFactory kernelExtensionFactory, Exception e )
			 {
				  Wrap( kernelExtensionFactory, e ).printStackTrace( @out );
			 }
		 }
	}

}