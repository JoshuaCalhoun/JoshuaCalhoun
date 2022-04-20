// Fill out your copyright notice in the Description page of Project Settings.


#include "Entities/Entity_Base.h"
#include "Kismet/KismetMathLibrary.h"

// Sets default values
AEntity_Base::AEntity_Base()
{
 	// Set this pawn to call Tick() every frame.  You can turn this off to improve performance if you don't need it.
	PrimaryActorTick.bCanEverTick = true;

}

// Called when the game starts or when spawned
void AEntity_Base::BeginPlay()
{
	Super::BeginPlay();
	
}

// Called every frame
void AEntity_Base::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);

}

// Called to bind functionality to input
void AEntity_Base::SetupPlayerInputComponent(UInputComponent* PlayerInputComponent)
{
	Super::SetupPlayerInputComponent(PlayerInputComponent);

}

void AEntity_Base::AddVelocity(FVector Direction, float Scale)
{
	AddedVelocity = UKismetMathLibrary().Multiply_VectorFloat(Direction, (Scale * GetWorld()->GetDeltaSeconds())) + AddedVelocity;
}

void AEntity_Base::ResolveMovement()
{
	UE_LOG(LogTemp, Warning, TEXT("Entity_Base::ResolveMovement being called"));
}

bool AEntity_Base::ChangeMovement(EMovement NewMovement)
{
	EMovement oldMovement = GetMovementMode();

	bool shouldChange = false;

	switch (NewMovement)
	{
	case EMovement::EM_Grounded:
		if (GetMovementMode() != EMovement::EM_Dashing)
			shouldChange = true;
		break;
	case EMovement::EM_Jumping:
		if (GetMovementMode() != EMovement::EM_Dashing)
			shouldChange = true;
		break;
	case EMovement::EM_Rising:
		if (GetMovementMode() != EMovement::EM_Dashing)
			shouldChange = true;
		break;
	case EMovement::EM_Falling:
		if (GetMovementMode() != EMovement::EM_Jumping
			)
			shouldChange = true;
		break;
	case EMovement::EM_Dashing:
		shouldChange = true;
		break;
	case EMovement::EM_WallSlide:
		if (GetMovementMode() == EMovement::EM_Falling)
			shouldChange = true;
		break;

	default: break;
	}

	if (shouldChange)
	{
		MovementMode = NewMovement;

		// Only perform this if the modes are different
		if (oldMovement != NewMovement)
		{
			if (oldMovement == EMovement::EM_WallSlide)
			{
				 bWallLanded = false;
			}
			if (NewMovement == EMovement::EM_WallSlide)
			{
				 bWallLanded = true;
				// Look into how to set timer and reset wall landed function.
				GetWorldTimerManager().SetTimer(TimerHandleResetWallLanded, this, &AEntity_Base::ResetWallLanded, 0.1f, false);
			}

			return true;
		}
		else
		{
			return false;
		}
	}
	else
	{
		return false;
	}

	
}

void AEntity_Base::Gravity()
{
	float fScaleValue = 0.0f;

	switch (GetMovementMode())
	{
	case EMovement::EM_Grounded:
		fScaleValue = fGravityAcceleration;
		break;
	case EMovement::EM_Rising:
		fScaleValue = fRisingGravityAccel;
		break;
	case EMovement::EM_Falling:
		fScaleValue = fGravityAcceleration;
		break;
	case EMovement::EM_WallSlide:
		fScaleValue = fGravityAcceleration;
		break;
	}

	AddVelocity(GetMovementMode() == EMovement::EM_WallSlide ? FVector(0.0f, fWallDirection, 0.0f) : FVector(0.0f, 0.0f, -1.0f), fScaleValue);
}

void AEntity_Base::Ungrounded()
{
	if (!GetIsInAir())
	{
		ChangeMovement(EMovement::EM_Falling);
	}
}

void AEntity_Base::Grounded()
{
	ChangeMovement(EMovement::EM_Grounded);
	RefreshMovements();
	GetWorldTimerManager().SetTimer(TimerHandleUngrounded, this, &AEntity_Base::Ungrounded, 0.000001f, false);
}

void AEntity_Base::RefreshMovements()
{
	iDashCounter = 0;
	iJumpCounter = 0;
}

void AEntity_Base::StartJump()
{
	// Local variable if we are allowed to jump
	bool bCanJump = false;

	// First check if we are not grounded
	if (GetMovementMode() == EMovement::EM_Grounded)
	{
		bCanJump = true;
	}
	else
	{
		// Check if we are touching a wall or not.
		if (bTouchingWall & bCanWallSlideJump)
		{
			bCanJump = true;
			bWallJumpInitial = true;
			Velocity = FVector(0.0f, 0.0f, 0.0f);
		}
		// Otherwise, check if we can double jump.
		else
		{
			if (GetIsInAir())
			{
				if (iJumpCounter < iMaxJumps)
				{
					iJumpCounter++;
					bCanJump = true;
				}
			}
		}
	}
	
	if (bCanJump)
	{
		fJumpTimer = 0.0f;
		ChangeMovement(EMovement::EM_Jumping);
	}
}

void AEntity_Base::Jumping()
{
	if (GetMovementMode() == EMovement::EM_Jumping)
	{
		// Add velocity for the jump
		AddVelocity(bWallJumpInitial ? FVector(0.0f, 0.0f, 1.0f) + FVector(0.0f, fWallDirection * -2.0, 0.0f) : FVector(0.0f, 0.0f, 1.0f), fJumpAcceleration);

		// Check jump timer and if we need to stop jumping.
		fJumpTimer = fJumpTimer + GetWorld()->GetDeltaSeconds();

		if (fJumpTimer >= fJumpMaxDuration)
		{
			StopJump();
		}

		if (fJumpTimer >= (fJumpMaxDuration / 2.0))
		{
			bWallJumpInitial = false;
		}
	}
}

void AEntity_Base::StopJump(bool bSharp)
{
	if (GetMovementMode() == EMovement::EM_Jumping)
	{
		bWallJumpInitial = false;
		ChangeMovement(EMovement::EM_Rising);
		if (bSharp)
		{
			AddVelocity(FVector(0.0f, 0.0f, -1.0f), UKismetMathLibrary().Abs(((Velocity.Z / GetWorld()->GetDeltaSeconds()) / 1.5f)));
		}
	}
}

void AEntity_Base::ResetWallLanded()
{
	bWallLanded = false;
	 
	// Clear the timer
	GetWorldTimerManager().ClearTimer(TimerHandleResetWallLanded);
}

EMovement AEntity_Base::GetMovementMode() const
{
	return MovementMode;
}

bool AEntity_Base::GetIsInAir() const
{
	return (GetMovementMode() == EMovement::EM_Jumping || 
			GetMovementMode() == EMovement::EM_Falling || 
			GetMovementMode() == EMovement::EM_Rising);
}

